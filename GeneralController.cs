using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplyTriadfs.Areas.Identity.Data;
using ApplyTriadfs.Core;
using ApplyTriadfs.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApplyTriadfs.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController : Controller
    {
        private readonly IRepositoryData repositoryData;
        private readonly IEmailSender emailSender;
        // private readonly Microsoft.AspNetCore.Identity.UserManager userManager;
        private readonly SignInManager<ApplyTriadfsUser> _signInManager;
        private readonly UserManager<ApplyTriadfsUser> _userManager;
        // private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public GeneralController(IRepositoryData repositoryData, IEmailSender emailSender, UserManager<ApplyTriadfsUser> userManager,
            SignInManager<ApplyTriadfsUser> signInManager)
        {
            this.repositoryData = repositoryData;
            this.emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;

        }

        [HttpGet]
        [Route("GetBrokerName")]
        public List<string> GetBrokerName(int id)
        {
            //   db.Database()

            return null;
        }

        [HttpGet]
        [Route("HomeLocationState")]
        public IActionResult GetHomeLocationState()
        {
            var item = repositoryData.GetHomeLocationState("Loan").ToList();
            return null;
        }




        public class UserMail
        {
            public string emailid { get; set; }
            public string pwd { get; set; }
            public string FirstName { get; set; }
            public string bestDescribe { get; set; }
            public string loanSeeking { get; set; }
        }

        public class ReturnResult
        {
            public bool success { get; set; }
            public string message { get; set; }
        }


        [HttpPost]
        [Route("GeneratePasswordOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePasswordOnlyAsync([FromBody] UserMail userMail)
        {
            var existUser = _userManager.FindByEmailAsync(userMail.emailid);
            if (existUser.Result != null)
            {
                var user = _userManager.FindByEmailAsync(userMail.emailid);

                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user.Result);
                string strGeneratePwd = GenerateRandomPassword();
                IdentityResult passwordChangeResult = await _userManager.ResetPasswordAsync(user.Result, resetToken, strGeneratePwd);

                string strBodyMsg = "<p>Please enter this password <code><b>" + strGeneratePwd + "</b></code> to proceed with your loan application.</p>";

                await emailSender.SendEmailAsync(userMail.emailid, "Submit new application", strBodyMsg);
                return Ok(new { success = true, message = "Pls check you mailbox, and enter generated password here in password textbox." });
            }
            else {
                return Ok(new { success = false, message = "Error while generating password." });
            }          


        }



        [HttpPost]
        [Route("GeneratePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePasswordAsync([FromBody] UserMail userMail)
        {
            var existUser = _userManager.FindByEmailAsync(userMail.emailid);
            if (existUser.Result == null)
            {
                string strGeneratePwd = GenerateRandomPassword();
                string strBodyMsg = "<p>Please enter this password <code><b>" + strGeneratePwd + "</b></code> to proceed with your loan application.</p>";

                // Create New User Account
                var user = new ApplyTriadfsUser { UserName = userMail.emailid, Email = userMail.emailid, FullName = userMail.FirstName };
                var result = await _userManager.CreateAsync(user, strGeneratePwd);
                if (result.Succeeded)
                {
                    await emailSender.SendEmailAsync(userMail.emailid, "Submit new application", strBodyMsg);
                    return Ok(new { success = true, message = "Pls check you mailbox, and enter generated password here in password textbox." });
                }

                return Ok(new { success = false, message = "Error occur" });
            }
            else
            {
                var user = _userManager.FindByEmailAsync(userMail.emailid);
                              
                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user.Result);
                string strGeneratePwd = GenerateRandomPassword();
                IdentityResult passwordChangeResult = await _userManager.ResetPasswordAsync(user.Result, resetToken, strGeneratePwd);

                string strBodyMsg = "<p>Please enter this password <code><b>" + strGeneratePwd + "</b></code> to proceed with your loan application.</p>";

                await emailSender.SendEmailAsync(userMail.emailid, "Submit new application", strBodyMsg);
                return Ok(new { success = true, message = "Pls check you mailbox, and enter generated password here in password textbox." });
            }

        }

        [HttpPost]
        [Route("VerifyPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPasswordAsync([FromBody] UserMail userMail)
        {
            bool result = await AutCreateAccountAsync(userMail);
            ReturnResult rst = new ReturnResult();
            rst.success = result;
            rst.message = " ";
            return Ok(rst);

        }

        [HttpPost]
        [Route("SavePage")]
        [ValidateAntiForgeryToken]
        public IActionResult SavePage([FromBody] object obj)
        {
            var aa = obj;
            ReturnResult rst = new ReturnResult();
            rst.success = true;
            rst.message = " ";
            return Ok(rst);

        }

        [HttpPost]
        [Route("UserLogin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLogin(UserMail userMail)
        {
            bool flagset = false;
            string strMessage = string.Empty;
            string strId = string.Empty;
            //  _logger.LogInformation("User created a new account with password.");
            var loggedIn = await _signInManager.PasswordSignInAsync(userMail.emailid, userMail.pwd, false, lockoutOnFailure: false);
            if (loggedIn.Succeeded)
            {
                flagset = true;
                strMessage = "New record created";
                strId = "0";
            }
            return Ok(new { status = flagset, message = strMessage, Id = strId });

        }

        public async Task<bool> AutCreateAccountAsync(UserMail userMail)
        {

            var user = new ApplyTriadfsUser { UserName = userMail.emailid, Email = userMail.emailid, FullName = userMail.FirstName };
            var result = await _userManager.CreateAsync(user, userMail.pwd);
            if (result.Succeeded)
            {
                //  _logger.LogInformation("User created a new account with password.");
                var loggedIn = await _signInManager.PasswordSignInAsync(userMail.emailid, userMail.pwd, false, lockoutOnFailure: false);
                if (loggedIn.Succeeded)
                {
                    return true;
                }
                else { return false; }
            }
            else { return false; }

        }
        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}
