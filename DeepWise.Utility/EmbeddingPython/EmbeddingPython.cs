using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddingPython
{
    namespace Python.Communication
    {
        using System;
        using System.IO;
        using System.Text;
        using System.Threading;
        using System.Windows;
        using System.IO.Pipes;
        using DotPy_PKG_Tool;
        using System.Management;
        using System.Security.Principal;
        using System.Threading.Tasks;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Linq;
        using Python.Communication.Pipe;
        using Python.Communication.Paser;
        using PythonCommand = Python.Communication.Paser.Command;
        using System.Collections.Generic;

        public class PythonProcessingExpansion
        {
            #region 類別參數
            System.Diagnostics.Process PyProcess = new System.Diagnostics.Process();
            string PipeName;
            PythonPipeServer Receivcer;
            PythonPipeClient Sender;
            Dictionary<string, object> TaskDict = new Dictionary<string, object>();
            #endregion
            #region 初始化
            public PythonProcessingExpansion(string PythonEnvPath, string EncryptedCode_FilePath, string PipeName, string PYTHONPATH_Extend = null, string SourceCodePath = null)
            {
                #region 原始代碼加密(僅測試使用，客戶電腦用完原始代碼請刪除)
                if (SourceCodePath != null)
                {
                    if (File.Exists(SourceCodePath))
                    {
                        DotPy_PKG.PKG_DotPy(SourceCodePath, EncryptedCode_FilePath);
                    }
                }
                #endregion

                #region 加密文件Exist檢查
                if (File.Exists(EncryptedCode_FilePath) == false)
                {
                    Console.WriteLine("Error: Paser not exist.");
                    throw new FileNotFoundException("Error: Paser not exist.", EncryptedCode_FilePath);
                }
                #endregion

                #region 解密並載入PythonCode
                string PyCodeString = DotPy_PKG.UnPKG_DotPy(EncryptedCode_FilePath);
                #endregion

                #region 生成臨時文件路徑(將原始碼寫成文件臨時給Python.exe讀取後即刪除)
                string tempPath = System.IO.Path.GetTempPath();
                string tempSaveFolderPath = System.IO.Path.Combine(tempPath, "env-523-22");
                string tempFileName = "temp-d45684138";
                string tempFilePath = System.IO.Path.Combine(tempSaveFolderPath, tempFileName);
                if (System.IO.Directory.Exists(tempSaveFolderPath) == false)
                {
                    System.IO.Directory.CreateDirectory(tempSaveFolderPath);
                }
                File.WriteAllText(tempFilePath, PyCodeString, encoding: Encoding.UTF8);
                // 等待檔案建立
                SpinWait.SpinUntil(new Func<bool>(() =>
                {
                    return File.Exists(tempFilePath);
                }), 10000);
                #endregion

                #region  建立通信管道
                this.PipeName = PipeName;
                this.Receivcer = new PythonPipeServer($"DEEPWISE_PY2CS_PIPE_{this.PipeName}");
                this.Sender = new PythonPipeClient($"DEEPWISE_CS2PY_PIPE_{this.PipeName}");
                #endregion

                #region 設定並建立Python Process 
                string EnvName = Path.GetFileName(PythonEnvPath);
                PyProcess.StartInfo.FileName = "cmd.exe";
                PyProcess.StartInfo.CreateNoWindow = false;
                PyProcess.StartInfo.UseShellExecute = true;
                PyProcess.StartInfo.RedirectStandardOutput = false;

                string set_EnvVar_Command;
                if (PYTHONPATH_Extend != null)
                {
                    String PATH = Environment.GetEnvironmentVariable("PYTHONPATH");
                    set_EnvVar_Command = String.Format(@"set IS_PIPE_MODE=TRUE && set PIPE_NAME={0} && set PYTHONPATH={1};{2}", this.PipeName, $"{PYTHONPATH_Extend.TrimEnd(';')}", PATH);
                    PyProcess.StartInfo.Arguments = $"/C \"echo 設定環境變數... && {set_EnvVar_Command} && activate {EnvName} && echo 建立Python程序... && python.exe {tempFilePath} && timeout 20\"";
                }
                else
                {
                    set_EnvVar_Command = String.Format(@"set IS_PIPEMODE=TRUE && set PIPE_NAME={0}", this.PipeName);
                    PyProcess.StartInfo.Arguments = $"/C \"echo 設定環境變數... && {set_EnvVar_Command} && activate {EnvName} && echo 建立Python程序... && python.exe {tempFilePath} && timeout 20\"";
                }

                PyProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                // 執行程序
                PyProcess.Start();
                #endregion

                #region 通信管道連線
                Task ServerPipeConnectTask = Task.Run(() =>
                {
                    CancellationTokenSource StopToken_R = new CancellationTokenSource();
                    this.Receivcer.Connect(StopToken_R);
                });
                Task ClientPipeConnectTask = Task.Run(() =>
                {
                    CancellationTokenSource StopToken_S = new CancellationTokenSource();
                    this.Sender.Connect(StopToken_S);
                });
                bool Finish = Task.WaitAll(new Task[] { ServerPipeConnectTask, ClientPipeConnectTask }, 5000);
                if (Finish == false)
                {
                    throw new Exception("Python NamedPipe connect timeout.");
                }
                else
                {
                    this.Receivcer.ReceiveDataEvent += ReceiveDataEventHabdle;
                    this.Receivcer.ConnectEvent += ConnectAndDisconnectHandle;
                    this.Receivcer.DisConnectEvent += ConnectAndDisconnectHandle;
                    this.Sender.ConnectEvent += ConnectAndDisconnectHandle;
                    this.Sender.DisConnectEvent += ConnectAndDisconnectHandle;
                }
                #endregion

                #region 刪除臨時檔案
                Thread.Sleep(3000);
                File.WriteAllText(tempFilePath, "", encoding: Encoding.UTF8);
                File.Delete(tempFilePath);
                #endregion
            }
            #endregion

            public void ConnectAndDisconnectHandle(object sender, Python.Communication.Pipe.ConnectEventArgs e)
            {
                Console.WriteLine($"{sender.GetType()}: IsConnected {e.IsConnected}");
            }

            #region Receive from Python
            public void ReceiveDataEventHabdle(object sender, PythonPipeServer.ReceiveDataEventArgs e)
            {
                #region 處理Python回傳
                JObject Return_PyDict = JObject.Parse(e.Data);
                string TaskName = Return_PyDict["TaskName"].Value<string>();
                int StateCode = Return_PyDict["StateCode"].Value<int>();
                if (StateCode == -1)
                {
                    string msg = Return_PyDict["StateCodeMessage"].Value<string>();
                    throw new Exception(msg);
                }
                else
                {
                    this.TaskDict[TaskName] = Return_PyDict["ReturnValue"];
                }
                #endregion
            }
            #endregion

            #region Send to Python
            public Task<JToken> CallPython(CommandSetting CommandClass, string TaskName = null, int timeout = -1)
            {
                var task = Task.Run(new Func<JToken>(() =>
                {
                    Python_Parser_Dict python_Parser_Dict = new Python_Parser_Dict(CommandClass, TaskName);
                    string CommandJsonString = JsonConvert.SerializeObject(python_Parser_Dict);
                    this.Sender.Write(CommandJsonString);
                    this.TaskDict[TaskName] = new EmptyObject();
                    bool isOK = SpinWait.SpinUntil(new Func<bool>(() =>
                    {
                        if (this.TaskDict[TaskName].GetType() != typeof(EmptyObject))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }), timeout);
                    if (isOK)
                    {
                        return (this.TaskDict[TaskName] as JToken);
                    }
                    else
                    {
                        throw new TimeoutException("CallPython timeout.");
                    }
                }));
                return task;
            }
            public Task<T> CallPython<T>(CommandSetting CommandClass, string TaskName = null, int timeout = -1)
            {
                var task = Task.Run(new Func<T>(() =>
                {
                    Python_Parser_Dict python_Parser_Dict = new Python_Parser_Dict(CommandClass, TaskName);
                    string CommandJsonString = JsonConvert.SerializeObject(python_Parser_Dict);
                    this.Sender.Write(CommandJsonString);
                    this.TaskDict[TaskName] = new EmptyObject();
                    bool isOK = SpinWait.SpinUntil(new Func<bool>(() =>
                    {
                        if (this.TaskDict[TaskName].GetType() != typeof(EmptyObject))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }), timeout);
                    if (isOK)
                    {
                        return (this.TaskDict[TaskName] as JToken).Value<T>();
                    }
                    else
                    {
                        throw new TimeoutException("CallPython timeout.");
                    }
                }));
                return task;
            }
            #endregion

            #region 釋放資源
            public void DisposeProcess()
            {
                KillProcessAndChildrens(this.PyProcess.Id);
                this.PyProcess.Dispose();
                this.PyProcess = null;
            }
            ~PythonProcessingExpansion()
            {
                KillProcessAndChildrens(this.PyProcess.Id);
                this.PyProcess.Dispose();
            }
            private static void KillProcessAndChildrens(int pid)
            {
                // Then kill parents.
                try
                {
                    ManagementObjectSearcher processSearcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
                    ManagementObjectCollection processCollection = processSearcher.Get();

                    // We must kill child processes first!
                    if (processCollection != null)
                    {
                        foreach (ManagementObject mo in processCollection)
                        {
                            KillProcessAndChildrens(Convert.ToInt32(mo["ProcessID"])); //kill child processes(also kills childrens of childrens etc.)
                        }
                    }
                    System.Diagnostics.Process proc = System.Diagnostics.Process.GetProcessById(pid);
                    if (!proc.HasExited) proc.Kill();
                }
                catch (ArgumentException)
                {
                    // Process already exited.
                }
            }
            #endregion

            public class EmptyObject { }
        }

        namespace Pipe
        {
            public class PythonPipeServer
            {
                #region 類別參數
                #region Event
                public delegate void ConnectEventHandler(object sender, ConnectEventArgs e);
                public event ConnectEventHandler ConnectEvent;

                public delegate void DisConnectEventHandler(object sender, ConnectEventArgs e);
                public event DisConnectEventHandler DisConnectEvent;
                public class ReceiveDataEventArgs : EventArgs
                {
                    public string Data { get; set; }
                }
                public delegate void ReceiveDataEventHandler(object sender, ReceiveDataEventArgs e);
                public event ReceiveDataEventHandler ReceiveDataEvent;
                #endregion
                #region 伺服器設定
                public string PipeName;
                NamedPipeServerStream server;
                CancellationTokenSource CancelConnectToken;
                #endregion
                #region 讀寫參數
                Encoding Encoding = Encoding.UTF8;
                private object ReadWriteLock = new object();
                #region 讀取器線程
                Task ReaderTask;
                bool StopFlag = false;
                #endregion
                #endregion
                #endregion

                public PythonPipeServer(string pipeName)
                {
                    this.PipeName = pipeName;
                    this.server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Message,
                        PipeOptions.None, 1024 * 1024, 1024 * 1024);
                    this.ReaderTask = new Task(this.Read);
                }
                ~PythonPipeServer()
                {
                    this.server?.Dispose();
                }
                public async Task Connect(CancellationTokenSource cancellationToken = null)
                {
                    if (this.server.IsConnected == false)
                    {
                        if (cancellationToken != null)
                        {
                            this.CancelConnectToken = cancellationToken;
                            var task = this.server?.WaitForConnectionAsync(cancellationToken.Token);
                            await task;
                        }
                        else
                        {
                            this.server?.WaitForConnection();
                        }
                        var para = new ConnectEventArgs();
                        if (this.server.IsConnected)
                        {
                            this.CancelConnectToken = null;
                            para.IsConnected = true;
                            this.ConnectEvent?.Invoke(this, para);
                            ReaderTask.Start();
                        }
                        else
                        {
                            para.IsConnected = false;
                            this.DisConnectEvent?.Invoke(this, para);
                        }
                    }
                }

                public void DisConnect()
                {
                    var para = new ConnectEventArgs();
                    if (this.server.IsConnected)
                    {
                        this.server.Disconnect();
                        para.IsConnected = false;
                        this.DisConnectEvent?.Invoke(this, para);
                    }
                    else
                    {
                        if (this.CancelConnectToken != null)
                        {
                            this.CancelConnectToken.Cancel();
                            para.IsConnected = false;
                            this.DisConnectEvent?.Invoke(this, para);
                        }
                    }
                }

                public void Read()
                {
                    if (this.server.IsConnected)
                    {
                        while (this.StopFlag == false)
                        {
                            lock (this.ReadWriteLock)
                            {
                                var stream = new StreamString(this.server, this.Encoding);
                                string msg = stream.ReadString();
                                ReceiveDataEventArgs para = new ReceiveDataEventArgs();
                                para.Data = msg;
                                this.ReceiveDataEvent?.Invoke(this, para);
                                //Console.WriteLine(msg);
                            }
                        }
                    }
                }
            }
            public class PythonPipeClient
            {
                #region 類別參數
                #region Event
                public delegate void ConnectEventHandler(object sender, ConnectEventArgs e);
                public event ConnectEventHandler ConnectEvent;

                public delegate void DisConnectEventHandler(object sender, ConnectEventArgs e);
                public event DisConnectEventHandler DisConnectEvent;
                #endregion
                #region 伺服器參數
                public string PipeName;
                NamedPipeClientStream client;
                CancellationTokenSource CancelConnectToken;
                #endregion
                #region 讀寫參數
                Encoding Encoding = Encoding.UTF8;
                private object ReadWriteLock = new object();
                #endregion
                #endregion

                public PythonPipeClient(string pipeName)
                {
                    this.PipeName = pipeName;
                    this.client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                }
                ~PythonPipeClient()
                {
                    this.client?.Dispose();
                }
                public async void Connect(CancellationTokenSource cancellationToken = null)
                {
                    if (this.client.IsConnected == false)
                    {
                        if (cancellationToken != null)
                        {
                            this.CancelConnectToken = cancellationToken;
                            var task = this.client?.ConnectAsync(cancellationToken.Token);
                            await task;
                        }
                        else
                        {
                            this.client?.Connect();
                        }
                        var para = new ConnectEventArgs();
                        if (this.client.IsConnected)
                        {
                            this.CancelConnectToken = null;
                            para.IsConnected = true;
                            this.ConnectEvent?.Invoke(this, para);
                        }
                        else
                        {
                            para.IsConnected = false;
                            this.DisConnectEvent?.Invoke(this, para);
                        }
                    }
                }
                public void DisConnect()
                {
                    var para = new ConnectEventArgs();
                    if (this.client.IsConnected)
                    {
                        this.client.Close();
                        para.IsConnected = false;
                        this.DisConnectEvent?.Invoke(this, para);
                    }
                    else
                    {
                        if (this.CancelConnectToken != null)
                        {
                            this.CancelConnectToken.Cancel();
                            para.IsConnected = false;
                            this.DisConnectEvent?.Invoke(this, para);
                        }
                    }
                }

                public void Write(string msg)
                {
                    if (this.client.IsConnected)
                    {
                        lock (this.ReadWriteLock)
                        {
                            var stream = new StreamString(this.client, this.Encoding);
                            stream.WriteString(msg);
                        }
                    }
                }
            }
            public class ConnectEventArgs : EventArgs
            {
                public bool IsConnected { get; set; }
            }
            public class StreamString
            {
                private Stream ioStream;
                private Encoding streamEncoding = Encoding.UTF8;

                public StreamString(Stream ioStream, Encoding encoding = null)
                {
                    this.ioStream = ioStream;
                    if (encoding != null)
                    {
                        streamEncoding = encoding;
                    }
                }

                public string ReadString()
                {
                    int len;
                    len = ioStream.ReadByte() * 256;
                    len += ioStream.ReadByte();
                    var inBuffer = new byte[len];
                    ioStream.Read(inBuffer, 0, len);
                    return streamEncoding.GetString(inBuffer);
                }

                public int WriteString(string outString)
                {
                    byte[] outBuffer = streamEncoding.GetBytes(outString);
                    int len = outBuffer.Length;
                    if (len > UInt16.MaxValue)
                    {
                        len = (int)UInt16.MaxValue;
                    }
                    ioStream.WriteByte((byte)(len / 256));
                    ioStream.WriteByte((byte)(len % 255));
                    ioStream.Write(outBuffer, 0, len);
                    ioStream.Flush();
                    return outBuffer.Length + 2;
                }
            }
        }

        namespace Paser
        {
            public class Python_Parser_Dict
            {
                public string TaskName = "None";

                public CommandSetting TaskParameter;

                public Python_Parser_Dict(CommandSetting commandSetting, string taskName)
                {
                    this.TaskName = taskName;
                    this.TaskParameter = commandSetting;
                }
            }

            public abstract class CommandSetting
            {
                [JsonProperty("Command_Name")]
                protected string Command_Name;
            }

            namespace Command
            {
                public class CreateClassObjectCommand : CommandSetting
                {
                    public string VariableName = "";
                    public string ClassName = "";
                    public JObject ClassParameter = new JObject();
                    public CreateClassObjectCommand(string VariableName, string ClassName, params (string, object)[] ClassParameter)
                    {
                        this.Command_Name = "CreateClassObject";
                        this.VariableName = VariableName;
                        this.ClassName = ClassName;
                        JObject ClassParameterJobject = new JObject();
                        foreach (var (key, value) in ClassParameter)
                        {
                            ClassParameterJobject.Add(key, JToken.FromObject(value));
                        }
                        this.ClassParameter = ClassParameterJobject;
                    }
                }
                public class DeleteClassObjectCommand : CommandSetting
                {
                    public string VariableName = "";
                    public DeleteClassObjectCommand(string VariableName)
                    {
                        this.Command_Name = "DeleteClassObject";
                        this.VariableName = VariableName;
                    }
                }
                public class RunClassFunctionCommand : CommandSetting
                {
                    public string VariableName = "";
                    public string FunctionName = "";
                    public JObject FunctionParameter = new JObject();
                    public RunClassFunctionCommand(string VariableName, string FunctionName, params (string, object)[] FunctionParameter)
                    {
                        this.Command_Name = "RunClassFunction";
                        this.VariableName = VariableName;
                        this.FunctionName = FunctionName;
                        JObject FunctionParameterJobject = new JObject();
                        foreach (var (key, value) in FunctionParameter)
                        {
                            FunctionParameterJobject.Add(key, JToken.FromObject(value));
                        }
                        this.FunctionParameter = FunctionParameterJobject;
                    }
                }
                public class RunStaticFunctionCommand : CommandSetting
                {
                    public string ClassName = "";
                    public string FunctionName = "";
                    public JObject FunctionParameter = new JObject();
                    public RunStaticFunctionCommand(string ClassName, string FunctionName, params (string, object)[] FunctionParameter)
                    {
                        this.Command_Name = "RunStaticFunction";
                        this.ClassName = ClassName;
                        this.FunctionName = FunctionName;
                        JObject FunctionParameterJobject = new JObject();
                        foreach (var (key, value) in FunctionParameter)
                        {
                            FunctionParameterJobject.Add(key, JToken.FromObject(value));
                        }
                        this.FunctionParameter = FunctionParameterJobject;
                    }
                }
            }
        }
    }

    namespace DotPy_PKG_Tool
    {
        using System;
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using System.Text;
        using System.Threading;
        using System.Threading.Tasks;
        using System.Security.Cryptography;
        using System.Windows;
        using Newtonsoft.Json;
        using System.Xml.Serialization;

        public class DotPy_PKG
        {
            /// <summary>
            /// only support utf-8
            /// </summary>
            /// <param name="dotPy_Path"></param>
            /// <returns></returns>
            public static bool PKG_DotPy(string dotPy_Path, string SavedFullPath)
            {
                dotPy_Path = Path.GetFullPath(dotPy_Path);
                SavedFullPath = Path.GetFullPath(SavedFullPath);
                if (File.Exists(dotPy_Path) == true)
                {
                    string py_string = File.ReadAllText(dotPy_Path, Encoding.UTF8);
                    PyCodeString dotPy_PKG_Class = new PyCodeString() { dot_py_code = py_string };
                    PyBaseConfig<PyCodeString> baseConfig = new PyBaseConfig<PyCodeString>(SavedFullPath);
                    if (!baseConfig.Save(dotPy_PKG_Class))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            public static string UnPKG_DotPy(string SavedFullPath)
            {
                SavedFullPath = Path.GetFullPath(SavedFullPath);
                string rtn_str = null;
                PyBaseConfig<PyCodeString> baseConfig = new PyBaseConfig<PyCodeString>(SavedFullPath);
                PyCodeString dotPy_PKG_Class = new PyCodeString();

                if (!baseConfig.Load(ref dotPy_PKG_Class))
                {
                    return rtn_str;
                }
                else
                {
                    rtn_str = dotPy_PKG_Class.dot_py_code;
                    return rtn_str;
                };
            }

            public class PyCodeString
            {
                public string dot_py_code = null;
            }
        }


        class PyBaseConfig<T>
        {
            #region Variable
            private string m_DefaultConfigFolder = @"C:\DeepWise_Program\";
            private string m_ConfigFullPath;
            private string m_ConfigRootPath;
            private string m_ConfigName;
            private string m_SubTitle = ".dat";
            private bool m_IsInit = false;
            #endregion

            #region Rijndael 演算法的 Managed
            // Managed version of the Rijndael Algorithm to encrypt and decrypt data
            private RijndaelManaged KeyManaged = new RijndaelManaged();
            private ICryptoTransform Encryptor;
            private ICryptoTransform Decryptor;
            #endregion

            protected string ConfigName
            {
                get => m_ConfigName;
                set => m_ConfigName = String.IsNullOrEmpty(value) ? "Default" : value;
            }

            /// <summary>
            /// Initialize class
            /// </summary>
            /// <param name="configName">When value is null or empty, set value as "Default"</param>
            public PyBaseConfig(string fullPathNames)
            {
                if (Path.IsPathRooted(fullPathNames))
                {
                    Setup(Path.GetDirectoryName(fullPathNames), Path.GetFileNameWithoutExtension(fullPathNames), Path.GetExtension(fullPathNames));
                }
            }

            private void Setup(string fileDirectory, string filename, string fileExtention)
            {
                m_ConfigRootPath = fileDirectory;
                ConfigName = filename;
                m_ConfigFullPath = Path.Combine(m_ConfigRootPath, ConfigName + fileExtention);

                // This default key pair had been definied at least from 2019/12/04.
                //SetupEncryptAndDecryptKey("dikj9517IJYD19cj", "87ds13IUNC23id56");
                SetupEncryptAndDecryptKey(@"u5PPo9OInIMqKBzh", @"Z1VkKaWFFxcSDFag");
                m_IsInit = true;
            }

            /// <summary>
            /// Setup 
            /// </summary>
            /// <param name="Key">The secret key to be used for the symmetric algorithm. The key size must be 128, 192, or 256 bits.</param>
            /// <param name="IV">The IV to be used for the symmetric algorithm.</param>
            /// <returns></returns>
            public void SetupEncryptAndDecryptKey(string Key, string IV)
            {
                this.KeyManaged = new RijndaelManaged
                {
                    Key = System.Text.ASCIIEncoding.ASCII.GetBytes(Key),
                    IV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV)
                };
                this.Encryptor = this.KeyManaged.CreateEncryptor();
                this.Decryptor = this.KeyManaged.CreateDecryptor();
            }

            public bool Save(object obj)
            {
                return Save(ConvertObjToType(obj));
            }

            /// <summary>
            /// Write object to flie that will ecrypt or not according to "isEncrypt" parameter.
            /// </summary>
            /// <param name="obj">Object to write</param>
            /// <param name="isEncrypt">flag for encrypt or not</param>
            /// <returns></returns>
            public bool Save(T obj)
            {
                // Use setting path.
                string filePath = m_ConfigFullPath;

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));

                if (dirInfo.Exists == true)
                {
                    WriteConfig(filePath, ref obj);
                    return true;
                }
                return false;
            }

            public bool Load(ref object obj)
            {
                if (obj == null) return false;

                T srcObj = ConvertObjToType(obj);

                bool isLoadOk = Load(ref srcObj);

                if (isLoadOk)
                {
                    obj = (object)srcObj;
                }

                return isLoadOk;
            }

            /// <summary>
            /// Load object from flie that will decrypt or not according to "isEncrypt" parameter.
            /// </summary>
            /// <param name="obj">Object to write</param>
            /// <param name="isEncrypt">flag for encrypt or not</param>
            /// <returns></returns>
            public bool Load(ref T obj)
            {
                // Use setting path.
                string filePath = m_ConfigFullPath;

                if (File.Exists(m_ConfigFullPath))
                {
                    obj = ReadConfig(m_ConfigFullPath, obj);
                    return true;
                }
                return false;
            }

            public void WriteConfig(string recipeFullPath, ref T obj)
            {
                // 讀取檔案至文件流
                // Read "Data File" as "File Stream" buffer

                using (var fileStream = new FileStream(recipeFullPath, FileMode.Create))
                {
                    // 加密流(將文件流加密至加密流)
                    // Encrypt "File Stream" into "Encrypted Stream" buffer
                    using (var encryptedStream = new CryptoStream(fileStream, Encryptor, CryptoStreamMode.Write))
                    {
                        // 記憶體資料流
                        // A backing store memory stream buffer be used for xml serializing
                        using (var memoryStream = new MemoryStream())
                        {
                            // 序列化這個物件進記憶體流
                            // Serializes "Class Object" into "Memory Stream"
                            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
                            memoryStream.Write(bytes, 0, bytes.Length);

                            // 從記憶體流寫入目標資料流
                            // Writes the "Memory Stream" to "Encrypted Streaem"
                            encryptedStream.Write(memoryStream.ToArray(), 0, Convert.ToInt32(memoryStream.Length));

                            // Flush and close
                            // Update "Encrypted Stream" data with the "MemoryStream", then clears the buffer
                            encryptedStream.FlushFinalBlock();
                        }
                    }
                }
            }

            public T ReadConfig(string recipeFullPath, T customObject)
            {
                // 讀取檔案至文件流
                // Read "Data File" as "File Stream" buffer
                using (var fileStream = new FileStream(recipeFullPath, FileMode.Open))
                {
                    // 解密流(將文件流解密至解密流)
                    // Decrypt "File Stream" into "Decrypted Stream" buffer
                    using (var decryptStream = new CryptoStream(fileStream, Decryptor, CryptoStreamMode.Read))
                    {
                        // 解密流寫入至記憶體流
                        //stream where decrypted contents will be stored
                        using (var decryptedStream = new MemoryStream())
                        {
                            int data;

                            while ((data = decryptStream.ReadByte()) != -1)
                                decryptedStream.WriteByte((byte)data);

                            //reset position in prep for reading
                            decryptedStream.Position = 0;

                            T OBJ = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(decryptedStream.ToArray()));
                            customObject = OBJ;
                        }
                    }
                }
                return customObject;
            }

            private T ConvertObjToType(object obj)
            {
                if (obj is T)
                {
                    return (T)obj;
                }
                try
                {
                    return (T)Convert.ChangeType(obj, typeof(T));
                }
                catch (InvalidCastException)
                {
                    return default(T);
                }
            }
        }
    }


}
