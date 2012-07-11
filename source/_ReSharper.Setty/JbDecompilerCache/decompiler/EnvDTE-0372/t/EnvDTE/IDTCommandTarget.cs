// Type: EnvDTE.IDTCommandTarget
// Assembly: EnvDTE, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Assembly location: C:\Windows\assembly\GAC\EnvDTE\8.0.0.0__b03f5f7f11d50a3a\EnvDTE.dll

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EnvDTE
{
  [TypeLibType((short) 4160)]
  [Guid("7EF39A3E-590D-4879-88D4-C9BE5BCFD92E")]
  [ComImport]
  public interface IDTCommandTarget
  {
    [DispId(1)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void QueryStatus([MarshalAs(UnmanagedType.BStr), In] string CmdName, [In] vsCommandStatusTextWanted NeededText, [In, Out] ref vsCommandStatus StatusOption, [MarshalAs(UnmanagedType.Struct), In, Out] ref object CommandText);

    [DispId(2)]
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void Exec([MarshalAs(UnmanagedType.BStr), In] string CmdName, [In] vsCommandExecOption ExecuteOption, [MarshalAs(UnmanagedType.Struct), In] ref object VariantIn, [MarshalAs(UnmanagedType.Struct), In, Out] ref object VariantOut, [In, Out] ref bool Handled);
  }
}
