using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp2.ModifyAttributes
{
    public enum ResultTypeEnum
    {
        Result_Type_Invalid = -1,
        Result_Type_Int32 = 0,
        Result_Type_Double,
        Result_Type_Boolean,
        Result_Type_Positive_Infinity,
        Result_Type_Negtive_Infinity,
        Result_Type_Byte_Array,
        Result_Type_String,
    }

    public enum ResultSignEnum
    {
        Result_Sign_Invalid = -1,
        Result_Sign_Equal = 0,
        Result_Sign_Greater_Than,
        Result_Sign_Less_Than,
        Result_Sign_Interval,
        Result_Sign_Regex,
        Result_Sign_Lambda,
        Result_NotValiad
    }

    public class SftpFileTransferParameters
    {
        public bool IsUploadFileToTerminal = true;
        public string FullFileNameTerminal;
        public string FullFileNameComputer;
    }

    public class SSHCommandClass
    {
        public string Command { get; set; } = "";
        public string Result { get; set; } = "";
        public ResultSignEnum ResultSign { get; set; } = ResultSignEnum.Result_Sign_Equal;
        public string Description { get; set; } = "";
        public int Delay { get; set; } = 0;
    }

    public class CheckSingleSftpClass
    {
        public string Description { get; set; }
        public SSHCommandClass SSHCmdBeforeDownload { get; set; }
        public SftpFileTransferParameters SftpParam { get; set; }
        public SSHCommandClass SSHCmdAfterDownload { get; set; }
    }

    public class AttributeValueFromControl
    {
    }

    public class ModifyAttributeValue
    {
        private int ConfigType;
        private string ConfigValue;
    }

    public class ConfigAttributeItem
    {
        public string ConfigName;
        public ModifyAttributeValue ConfigValue;
    }

    public class ModifyConfigFileItem
    {
        public string FileName { get; set; }

        public CheckSingleSftpClass DownloadToTerminal { get; set; }

        public List<ConfigAttributeItem> ConfigAttributeList { get; set; }
    }

    public class ModifyConfigFilesClass
    {
        public List<ModifyConfigFileItem> ModifyConfigFileList { get; set; }
    }
}