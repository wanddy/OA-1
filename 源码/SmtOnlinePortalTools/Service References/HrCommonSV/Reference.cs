﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.34011
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SmtOnlinePortalTools.HrCommonSV {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="", ConfigurationName="HrCommonSV.HrCommonService")]
    public interface HrCommonService {
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:HrCommonService/GetAppConfigByName", ReplyAction="urn:HrCommonService/GetAppConfigByNameResponse")]
        string GetAppConfigByName(string AppConfigName);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:HrCommonService/GetEntityProNameByEnityName", ReplyAction="urn:HrCommonService/GetEntityProNameByEnityNameResponse")]
        string[] GetEntityProNameByEnityName(string entityName, string masterIDName);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:HrCommonService/CustomerQuery", ReplyAction="urn:HrCommonService/CustomerQueryResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(string[]))]
        object CustomerQuery(string Sqlstring);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface HrCommonServiceChannel : SmtOnlinePortalTools.HrCommonSV.HrCommonService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class HrCommonServiceClient : System.ServiceModel.ClientBase<SmtOnlinePortalTools.HrCommonSV.HrCommonService>, SmtOnlinePortalTools.HrCommonSV.HrCommonService {
        
        public HrCommonServiceClient() {
        }
        
        public HrCommonServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public HrCommonServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public HrCommonServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public HrCommonServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetAppConfigByName(string AppConfigName) {
            return base.Channel.GetAppConfigByName(AppConfigName);
        }
        
        public string[] GetEntityProNameByEnityName(string entityName, string masterIDName) {
            return base.Channel.GetEntityProNameByEnityName(entityName, masterIDName);
        }
        
        public object CustomerQuery(string Sqlstring) {
            return base.Channel.CustomerQuery(Sqlstring);
        }
    }
}
