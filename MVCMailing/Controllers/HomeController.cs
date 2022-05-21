using System.Diagnostics;
using System.Security.Principal;
using EmailValidation;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCMailing.Models;
using MVCMailing.Services;
using Routes.Controllers;

namespace MVCMailing.Controllers;

public class HomeController : Controller
{
    private readonly IEmailService _emailService;
    
    public HomeController(IEmailService emailService)
    {
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
    
    public IActionResult Index(string? error)
    {
        return View(model: error);
    }
    
    
    public async Task<IActionResult> InBox()
    {
        if (!_emailService.LoginCred.IsValid) return Home.Login("You need to login first").Redirect(this); 
        var messages = await _emailService.RetrieveEmails();
        return View(model: messages);
    }
    
    [HttpGet]
    public IActionResult Login(string error)
    {
        return View(model: error);
    }

    [HttpPost]
    public IActionResult LoginPost(EmailLoginVm emailLoginVm)
    {
        if (!emailLoginVm.IsValid) return Home.Index("Invalid Login Data").Redirect(this);
        
        _emailService.LoginCred = emailLoginVm;

        if (emailLoginVm.Google) return RedirectToAction("LoginGoogle", "Home");
        return Home.Index(null).Redirect(this);
    }

    [HttpGet]
    [GoogleScopedAuthorize(GmailService.ScopeConstants.MailGoogleCom)]
    public IActionResult LoginGoogle()
    {
        return Home.Index(null).Redirect(this);
    }

    [HttpGet]
    public IActionResult Send()
    {
        if (!_emailService.LoginCred.IsValid) return Home.Login("You need to login first").Redirect(this);
        return View();
    }

    [HttpPost]
    public IActionResult SendPost(EmailMessageVm message)
    {
        if (!_emailService.LoginCred.IsValid) return Home.Login("You need to login first").Redirect(this);
        if (!message.IsValidSend) return Home.Index("Invalid Send Message Data").Redirect(this);
        _emailService.SendEmail(message);
        return Home.Index(null).Redirect(this);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(uint messageUid)
    {
        if (!_emailService.LoginCred.IsValid) return Home.Login("You need to login first").Redirect(this);
        var deleteSuccess = await _emailService.DeleteEmail(messageUid);
        string? error = deleteSuccess ? null : "Encountered an error while deleting messages";
        return Home.Index(error).Redirect(this);
    }
}