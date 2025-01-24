using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace ERPCx86;

public class Mem86 {

    private IntPtr pHandle;
    private ProcessModule mainModule;
    private Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();
    public Process p;

    public bool LoadProcess(int pid) {
        try {
            CloseProcess();

            p = Process.GetProcessById(pid);
            if (p == null) return false;

            pHandle = OpenProcess(2035711u, bInheritHandle: true, p.Id);
            mainModule = p.MainModule;

            modules.Clear();
            foreach (ProcessModule module in p.Modules) {
                modules.Add(module.ModuleName, module.BaseAddress);
            }

            return true;
        } catch {
            return false;
        }
    }

    public void CloseProcess() {
        if (p == null || pHandle == null) return;
        CloseHandle(pHandle);
        modules.Clear();
        p = null;
    }

    public UIntPtr GetCode(string text, int size = 8) {
        if (text == "") {
            return UIntPtr.Zero;
        }

        if (text.Contains(" ")) {
            text = text.Replace(" ", string.Empty);
        }

        if (!text.Contains("+") && !text.Contains(",")) {
            return new UIntPtr(Convert.ToUInt32(text, 16));
        }

        string text2 = text;
        checked {
            if (text.Contains("+")) {
                text2 = text.Substring(text.IndexOf('+') + 1);
            }

            byte[] array = new byte[size];
            if (Enumerable.Contains(text2, ',')) {
                List<int> list = new List<int>();
                string[] array2 = text2.Split(new char[1] { ',' });
                string[] array3 = array2;
                foreach (string text3 in array3) {
                    string text4 = text3;
                    if (text3.Contains("0x")) {
                        text4 = text3.Replace("0x", "");
                    }

                    int num;
                    if (!text3.Contains("-")) {
                        num = int.Parse(text4, NumberStyles.AllowHexSpecifier);
                    } else {
                        text4 = text4.Replace("-", "");
                        num = int.Parse(text4, NumberStyles.AllowHexSpecifier);
                        num *= -1;
                    }

                    list.Add(num);
                }

                int[] array4 = list.ToArray();
                if (text.Contains("base") || text.Contains("main")) {
                    ReadProcessMemory(pHandle, (UIntPtr)(ulong)((int)mainModule.BaseAddress + array4[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
                } else if (!text.Contains("base") && !text.Contains("main") && text.Contains("+")) {
                    string[] array5 = text.Split(new char[1] { '+' });
                    IntPtr intPtr = IntPtr.Zero;
                    if (!array5[0].ToLower().Contains(".dll") && !array5[0].ToLower().Contains(".exe") && !array5[0].ToLower().Contains(".bin")) {
                        string text5 = array5[0];
                        if (text5.Contains("0x")) {
                            text5 = text5.Replace("0x", "");
                        }

                        intPtr = (IntPtr)int.Parse(text5, NumberStyles.HexNumber);
                    } else {
                        try {
                            intPtr = modules[array5[0]];
                        } catch {
                            Debug.WriteLine("Module " + array5[0] + " was not found in module list!");
                            Debug.WriteLine("Modules: " + string.Join(",", modules));
                        }
                    }

                    ReadProcessMemory(pHandle, (UIntPtr)(ulong)((int)intPtr + array4[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
                } else {
                    ReadProcessMemory(pHandle, (UIntPtr)(ulong)array4[0], array, (UIntPtr)(ulong)size, IntPtr.Zero);
                }

                uint num2 = BitConverter.ToUInt32(array, 0);
                UIntPtr uIntPtr = (UIntPtr)0uL;
                for (int j = 1; j < array4.Length; j++) {
                    uIntPtr = new UIntPtr(Convert.ToUInt32(unchecked((long)num2) + unchecked((long)array4[j])));
                    ReadProcessMemory(pHandle, uIntPtr, array, (UIntPtr)(ulong)size, IntPtr.Zero);
                    num2 = BitConverter.ToUInt32(array, 0);
                }

                return uIntPtr;
            }

            int num3 = Convert.ToInt32(text2, 16);
            IntPtr intPtr2 = IntPtr.Zero;
            if (text.ToLower().Contains("base") || text.ToLower().Contains("main")) {
                intPtr2 = mainModule.BaseAddress;
            } else if (!text.ToLower().Contains("base") && !text.ToLower().Contains("main") && text.Contains("+")) {
                string[] array6 = text.Split(new char[1] { '+' });
                if (!array6[0].ToLower().Contains(".dll") && !array6[0].ToLower().Contains(".exe") && !array6[0].ToLower().Contains(".bin")) {
                    string text6 = array6[0];
                    if (text6.Contains("0x")) {
                        text6 = text6.Replace("0x", "");
                    }

                    intPtr2 = (IntPtr)int.Parse(text6, NumberStyles.HexNumber);
                } else {
                    try {
                        intPtr2 = modules[array6[0]];
                    } catch {
                        Debug.WriteLine("Module " + array6[0] + " was not found in module list!");
                        Debug.WriteLine("Modules: " + string.Join(",", modules));
                    }
                }
            } else {
                intPtr2 = modules[text.Split(new char[1] { '+' })[0]];
            }

            return (UIntPtr)(ulong)((int)intPtr2 + num3);
        }
    }

    public string LoadCode(string name, string file) {
        StringBuilder stringBuilder = new StringBuilder(1024);
        if (file != "") {
            GetPrivateProfileString("codes", name, "", stringBuilder, checked((uint)stringBuilder.Capacity), file);
        } else {
            stringBuilder.Append(name);
        }

        return stringBuilder.ToString();
    }

    public string ReadString(string code, int length = 32) {
        byte[] array = new byte[length];
        UIntPtr code2 = GetCode(code);
        if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)checked((ulong)length), IntPtr.Zero)) {
            return Encoding.UTF8.GetString(array).Split(new char[1])[0];
        }

        return "";
    }

    public int ReadInt(string code) {
        byte[] array = new byte[4];
        UIntPtr code2 = GetCode(code);
        if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)4uL, IntPtr.Zero)) {
            return BitConverter.ToInt32(array, 0);
        }

        return 0;
    }

    public long ReadLong(string code) {
        byte[] array = new byte[16];
        UIntPtr code2 = GetCode(code);
        if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)16uL, IntPtr.Zero)) {
            return BitConverter.ToInt64(array, 0);
        }

        return 0L;
    }

    public float ReadFloat(string code) {
        byte[] array = new byte[4];
        UIntPtr code2 = GetCode(code);
        try {
            if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)4uL, IntPtr.Zero)) {
                float num = BitConverter.ToSingle(array, 0);
                float result = num;
                result = (float)Math.Round(num, 2);
            }

            return 0f;
        } catch {
            return 0f;
        }
    }

    public double ReadDouble(string code) {
        byte[] array = new byte[8];
        UIntPtr code2 = GetCode(code);
        try {
            if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)8uL, IntPtr.Zero)) {
                double num = BitConverter.ToDouble(array, 0);
                double result = num;
                result = Math.Round(num, 2);
            }

            return 0.0;
        } catch {
            return 0.0;
        }
    }

    public int ReadByte(string code, string file = "") {
        byte[] array = new byte[1];
        UIntPtr code2 = GetCode(code);
        if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)1uL, IntPtr.Zero)) {
            return array[0];
        }

        return 0;
    }

    [DllImport("kernel32.dll")]
    public static extern int CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] IntPtr lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);
}

