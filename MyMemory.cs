using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace EverythingRichPresence {

    public class MyMemory {

        private readonly IntPtr pHandle;
        private readonly ProcessModule mainModule;
        private readonly Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();
        private readonly bool is64Bit;
        public Process p;

        public MyMemory(Process p) {
            this.p = p;
            pHandle = OpenProcess(2035711u, bInheritHandle: true, p.Id);
            mainModule = p.MainModule;

            foreach (ProcessModule module in p.Modules) {
                modules.Add(module.ModuleName, module.BaseAddress);
            }

            is64Bit = IsWow64Process(pHandle, out var lpSystemInfo) && !lpSystemInfo;
        }

        public void CloseProcess() {
            CloseHandle(pHandle);
            p = null;
        }

        public UIntPtr GetCode(string name, string path = "", int size = 8) {
            if (is64Bit) {
                if (size == 8) {
                    size = 16;
                }
                return Get64BitCode(name, path, 16);
            }

            string text = !(path != "") ? name : LoadCode(name, path);
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

                        int num = 0;
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

        public UIntPtr Get64BitCode(string name, string path = "", int size = 16) {
            string text = "";
            text = ((!(path != "")) ? name : LoadCode(name, path));
            if (text == "") {
                return UIntPtr.Zero;
            }

            if (text.Contains(" ")) {
                text.Replace(" ", string.Empty);
            }

            string text2 = text;
            checked {
                if (text.Contains("+")) {
                    text2 = text.Substring(text.IndexOf('+') + 1);
                }

                byte[] array = new byte[size];
                if (!text.Contains("+") && !text.Contains(",")) {
                    return new UIntPtr(Convert.ToUInt64(text, 16));
                }

                if (Enumerable.Contains(text2, ',')) {
                    List<long> list = new List<long>();
                    string[] array2 = text2.Split(new char[1] { ',' });
                    string[] array3 = array2;
                    foreach (string text3 in array3) {
                        string text4 = text3;
                        if (text3.Contains("0x")) {
                            text4 = text3.Replace("0x", "");
                        }

                        long num = 0L;
                        if (!text3.Contains("-")) {
                            num = long.Parse(text4, NumberStyles.AllowHexSpecifier);
                        } else {
                            text4 = text4.Replace("-", "");
                            num = long.Parse(text4, NumberStyles.AllowHexSpecifier);
                            num *= -1;
                        }

                        list.Add(num);
                    }

                    long[] array4 = list.ToArray();
                    if (text.Contains("base") || text.Contains("main")) {
                        ReadProcessMemory(pHandle, (UIntPtr)(ulong)((long)mainModule.BaseAddress + array4[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
                    } else if (!text.Contains("base") && !text.Contains("main") && text.Contains("+")) {
                        string[] array5 = text.Split(new char[1] { '+' });
                        IntPtr intPtr = IntPtr.Zero;
                        if (!array5[0].ToLower().Contains(".dll") && !array5[0].ToLower().Contains(".exe") && !array5[0].ToLower().Contains(".bin")) {
                            intPtr = (IntPtr)long.Parse(array5[0], NumberStyles.HexNumber);
                        } else {
                            try {
                                intPtr = modules[array5[0]];
                            } catch {
                                Debug.WriteLine("Module " + array5[0] + " was not found in module list!");
                                Debug.WriteLine("Modules: " + string.Join(",", modules));
                            }
                        }

                        ReadProcessMemory(pHandle, (UIntPtr)(ulong)((long)intPtr + array4[0]), array, (UIntPtr)(ulong)size, IntPtr.Zero);
                    } else {
                        ReadProcessMemory(pHandle, (UIntPtr)(ulong)array4[0], array, (UIntPtr)(ulong)size, IntPtr.Zero);
                    }

                    long num2 = BitConverter.ToInt64(array, 0);
                    UIntPtr uIntPtr = (UIntPtr)0uL;
                    for (int j = 1; j < array4.Length; j++) {
                        uIntPtr = new UIntPtr(Convert.ToUInt64(num2 + array4[j]));
                        ReadProcessMemory(pHandle, uIntPtr, array, (UIntPtr)(ulong)size, IntPtr.Zero);
                        num2 = BitConverter.ToInt64(array, 0);
                    }

                    return uIntPtr;
                }

                long num3 = Convert.ToInt64(text2, 16);
                IntPtr intPtr2 = IntPtr.Zero;
                if (text.Contains("base") || text.Contains("main")) {
                    intPtr2 = mainModule.BaseAddress;
                } else if (!text.Contains("base") && !text.Contains("main") && text.Contains("+")) {
                    string[] array6 = text.Split(new char[1] { '+' });
                    if (!array6[0].ToLower().Contains(".dll") && !array6[0].ToLower().Contains(".exe") && !array6[0].ToLower().Contains(".bin")) {
                        string text5 = array6[0];
                        if (text5.Contains("0x")) {
                            text5 = text5.Replace("0x", "");
                        }

                        intPtr2 = (IntPtr)long.Parse(text5, NumberStyles.HexNumber);
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

                return (UIntPtr)(ulong)((long)intPtr2 + num3);
            }
        }

        public string LoadCode(string name, string file) {
            StringBuilder stringBuilder = new StringBuilder(1024);
            if (file != "") {
                uint privateProfileString = GetPrivateProfileString("codes", name, "", stringBuilder, checked((uint)stringBuilder.Capacity), file);
            } else {
                stringBuilder.Append(name);
            }

            return stringBuilder.ToString();
        }

        public string ReadString(string code, string file = "", int length = 32, bool zeroTerminated = true, Encoding stringEncoding = null) {
            if (stringEncoding == null) {
                stringEncoding = Encoding.UTF8;
            }

            byte[] array = new byte[length];
            UIntPtr code2 = GetCode(code, file);
            if (ReadProcessMemory(pHandle, code2, array, (UIntPtr)checked((ulong)length), IntPtr.Zero)) {
                return zeroTerminated ? stringEncoding.GetString(array).Split(new char[1])[0] : stringEncoding.GetString(array);
            }

            return "";
        }

        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32")]
        public static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

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
}
