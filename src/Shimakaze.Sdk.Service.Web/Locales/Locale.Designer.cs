﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shimakaze.WebServer.Locales {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Locale {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Locale() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Shimakaze.WebServer.Locales.Locale", typeof(Locale).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 Oops! We are unable to respond to your request 的本地化字符串。
        /// </summary>
        public static string Err400 {
            get {
                return ResourceManager.GetString("Err400", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Oops! We can&apos;t find the content you requested 的本地化字符串。
        /// </summary>
        public static string Err404 {
            get {
                return ResourceManager.GetString("Err404", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Server successfully responds to the request 的本地化字符串。
        /// </summary>
        public static string Log200 {
            get {
                return ResourceManager.GetString("Log200", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The server could not process the request correctly 的本地化字符串。
        /// </summary>
        public static string Log400 {
            get {
                return ResourceManager.GetString("Log400", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Server cannot find the requested address 的本地化字符串。
        /// </summary>
        public static string Log404 {
            get {
                return ResourceManager.GetString("Log404", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 An exception was thrown inside the server!\n {0} 的本地化字符串。
        /// </summary>
        public static string LogExceptionThrow {
            get {
                return ResourceManager.GetString("LogExceptionThrow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 There was an exception inside the server that could not be handled \n{0} 的本地化字符串。
        /// </summary>
        public static string LogFatalExceptionThrow {
            get {
                return ResourceManager.GetString("LogFatalExceptionThrow", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Found Servlet {0} 的本地化字符串。
        /// </summary>
        public static string LogFindServlet {
            get {
                return ResourceManager.GetString("LogFindServlet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The server received the request {0}: &quot;{1}&quot; 的本地化字符串。
        /// </summary>
        public static string LogServerGetRequest {
            get {
                return ResourceManager.GetString("LogServerGetRequest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The server is processing the request 的本地化字符串。
        /// </summary>
        public static string LogServerHandler {
            get {
                return ResourceManager.GetString("LogServerHandler", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Listen to requests from &quot;{0}&quot; 的本地化字符串。
        /// </summary>
        public static string LogServerListenAt {
            get {
                return ResourceManager.GetString("LogServerListenAt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The server is listening 的本地化字符串。
        /// </summary>
        public static string LogServerListening {
            get {
                return ResourceManager.GetString("LogServerListening", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Starting service 的本地化字符串。
        /// </summary>
        public static string LogServerStart {
            get {
                return ResourceManager.GetString("LogServerStart", resourceCulture);
            }
        }
    }
}
