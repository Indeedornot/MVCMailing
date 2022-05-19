using System.Diagnostics;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;
using MVCMailing.Models;
using MVCMailing.Services;
using Routes.Controllers;

namespace MVCMailing.Controllers;

public class HomeController : Controller
{
    private readonly IGoogleAuthProvider _auth;
    private readonly IEmailService _emailService;

    public HomeController(IGoogleAuthProvider auth, IEmailService emailService)
    {
        _auth = auth;
        _emailService = emailService;
    }
    
    #region privacy & error
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
    
    public IActionResult Privacy()
    {
        return View();
    }
    #endregion
    
    public async Task<IActionResult> Index()
    {
        return View();
    }
    
    [GoogleScopedAuthorize(GmailService.ScopeConstants.MailGoogleCom)]
    public async Task<IActionResult> InBox()
    {
        var credential = await _auth.GetCredentialAsync();
        var messages = await _emailService.RetrieveEmails(credential);
        return View(messages);
    }

    public IActionResult Send()
    {
        return View();
    }

    public async Task<IActionResult> SendPost(SendModel? sendModel)
    {
        if (sendModel is null) return Problem();
        var credential = await _auth.GetCredentialAsync();
        _ = _emailService.SendEmail(credential, sendModel);
        return Home.Index().Redirect(this);
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login()
    {
        
    }
}