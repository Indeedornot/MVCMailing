using System.Diagnostics;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MailKit;
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
    
    
    public async Task<IActionResult> InBox()
    {
        if (!_emailService.loginCred.IsValid) return Home.Login().Redirect(this); 
        var messages = await _emailService.RetrieveEmails();
        return View(messages);
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult LoginPost(EmailLoginVm emailLoginVm)
    {
        _emailService.loginCred = emailLoginVm;
        return Home.Index().Redirect(this);
    }

    [HttpGet]
    public IActionResult Send()
    {
        if (!_emailService.loginCred.IsValid) return Home.Login().Redirect(this);
        return View();
    }

    [HttpPost]
    public IActionResult SendPost(EmailMessageVm message)
    {
        if (!_emailService.loginCred.IsValid) return Home.Login().Redirect(this);
        if (!message.IsValidSend) return Home.Send().Redirect(this);
        _emailService.SendEmail(message);
        return Home.Index().Redirect(this);
    }
}