﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.2443
//
//     Changes to this file may cause incorrect behavior and will be lost if 
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

// 
// このソース コードは Microsoft.VSDesigner、バージョン 1.1.4322.2443 によって自動生成されました。
// 
namespace is2EXsyukka.is2EXlogout {
    using System.Diagnostics;
    using System.Xml.Serialization;
    using System;
    using System.Web.Services.Protocols;
    using System.ComponentModel;
    using System.Web.Services;
    
    
    /// <remarks/>
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="Service1Soap", Namespace="http://Walkthrough/XmlWebServices/")]
    public class Service1 : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        /// <remarks/>
        public Service1() {
            this.Url = "http://localhost/is2ex/is2EXlogout/Service1.asmx";
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://Walkthrough/XmlWebServices/LogOut", RequestNamespace="http://Walkthrough/XmlWebServices/", ResponseNamespace="http://Walkthrough/XmlWebServices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public int LogOut(string sLog) {
            object[] results = this.Invoke("LogOut", new object[] {
                        sLog});
            return ((int)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginLogOut(string sLog, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("LogOut", new object[] {
                        sLog}, callback, asyncState);
        }
        
        /// <remarks/>
        public int EndLogOut(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((int)(results[0]));
        }
    }
}
