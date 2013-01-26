﻿using System;
using JabbR.Infrastructure;
using JabbR.Models;
using JabbR.Services;
using JabbR.ViewModels;
using Nancy;
using Nancy.Cookies;

namespace JabbR.Nancy
{
    public class AccountModule : NancyModule
    {
        public AccountModule(IApplicationSettings applicationSettings,
                             IAuthenticationTokenService authenticationTokenService,
                             IMembershipService membershipService,
                             IJabbrRepository repository)
            : base("/account")
        {
            Get["/"] = _ =>
            {
                ChatUser user = repository.GetUserById(Context.CurrentUser.UserName);

                return View["index", new ProfilePageViewModel(user)];
            };

            Get["/login"] = _ => View["login", applicationSettings.AuthenticationMode];

            Post["/login"] = param =>
            {
                string name = Request.Form.username;
                string password = Request.Form.password;

                if (String.IsNullOrEmpty(name))
                {
                    ModelValidationResult.AddError("name", "Name is required");
                }

                if (String.IsNullOrEmpty(password))
                {
                    ModelValidationResult.AddError("password", "Password is required");
                }

                try
                {
                    if (ModelValidationResult.IsValid)
                    {
                        ChatUser user = membershipService.AuthenticateUser(name, password);
                        return this.CompleteLogin(authenticationTokenService, user);
                    }
                    else
                    {
                        return View["login", applicationSettings.AuthenticationMode];
                    }
                }
                catch (Exception ex)
                {
                    ModelValidationResult.AddError("_FORM", ex.Message);
                    return View["login", applicationSettings.AuthenticationMode];
                }
            };

            Post["/logout"] = _ =>
            {
                var response = Response.AsJson(new { success = true });

                response.AddCookie(new NancyCookie(Constants.UserTokenCookie, null)
                {
                    Expires = DateTime.Now.AddDays(-1)
                });

                return response;
            };

            Get["/register"] = _ => View["register"];

            Post["/create"] = _ =>
            {
                string name = Request.Form.username;
                string email = Request.Form.email;
                string password = Request.Form.password;
                string confirmPassword = Request.Form.confirmPassword;

                if (String.IsNullOrEmpty(name))
                {
                    ModelValidationResult.AddError("name", "Name is required");
                }

                if (String.IsNullOrEmpty(email))
                {
                    ModelValidationResult.AddError("email", "Email is required");
                }

                if (String.IsNullOrEmpty(password))
                {
                    ModelValidationResult.AddError("password", "Password is required");
                }

                if (!String.Equals(password, confirmPassword))
                {
                    ModelValidationResult.AddError("confirmPassword", "Passwords don't match");
                }

                try
                {
                    if (ModelValidationResult.IsValid)
                    {
                        ChatUser user = membershipService.AddUser(name, email, password);
                        return this.CompleteLogin(authenticationTokenService, user);
                    }
                    else
                    {
                        return View["register", ModelValidationResult];
                    }
                }
                catch(Exception ex)
                {
                    ModelValidationResult.AddError("_FORM", ex.Message);
                    return View["register", ModelValidationResult];
                }
            };
        }
    }
}