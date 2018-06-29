﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client.InformationPolicy;
using OfficeDevPnP.Core.Entities;
using OfficeDevPnP.Core.Utilities.Async;

namespace Microsoft.SharePoint.Client
{

    /// <summary>
    /// Class that deals with information management features
    /// </summary>
    public static partial class InformationManagementExtensions
    {

        /// <summary>
        /// Does this web have a site policy applied?
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>True if a policy has been applied, false otherwise</returns>
        public static bool HasSitePolicyApplied(this Web web)
        {
#if ONPREMISES
            return web.HasSitePolicyAppliedImplementation();
#else
            return Task.Run(() => web.HasSitePolicyAppliedImplementation()).GetAwaiter().GetResult();
#endif
        }
#if !ONPREMISES
        /// <summary>
        /// Does this web have a site policy applied?
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>True if a policy has been applied, false otherwise</returns>
        public static async Task<bool> HasSitePolicyAppliedAsync(this Web web)
        {
            await new SynchronizationContextRemover();
            return await web.HasSitePolicyAppliedImplementation();
        }
#endif
        /// <summary>
        /// Does this web have a site policy applied?
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>True if a policy has been applied, false otherwise</returns>
#if ONPREMISES
        private static bool HasSitePolicyAppliedImplementation(this Web web)
#else
        private static async  Task<bool> HasSitePolicyAppliedImplementation(this Web web)
#endif
        {
            var hasSitePolicyApplied = ProjectPolicy.DoesProjectHavePolicy(web.Context, web);
#if ONPREMISES
            web.Context.ExecuteQueryRetry();
#else
            await web.Context.ExecuteQueryRetryAsync();
#endif
            return hasSitePolicyApplied.Value;
        }

        /// <summary>
        /// Gets the site expiration date
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>DateTime value holding the expiration date, DateTime.MinValue in case there was no policy applied</returns>
        public static DateTime GetSiteExpirationDate(this Web web)
        {
#if ONPREMISES
            return web.GetSiteExpirationDateImplementation();
#else
            return Task.Run(() => web.GetSiteExpirationDateImplementation()).GetAwaiter().GetResult();
#endif
        }
#if !ONPREMISES
        /// <summary>
        /// Gets the site expiration date
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>DateTime value holding the expiration date, DateTime.MinValue in case there was no policy applied</returns>
        public static async Task<DateTime> GetSiteExpirationDateAsync(this Web web)
        {
            await new SynchronizationContextRemover();
            return await web.GetSiteExpirationDateImplementation();
        }
#endif
        /// <summary>
        /// Gets the site expiration date
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>DateTime value holding the expiration date, DateTime.MinValue in case there was no policy applied</returns>
#if ONPREMISES
        private static DateTime GetSiteExpirationDateImplementation(this Web web)
#else
        private static async Task<DateTime> GetSiteExpirationDateImplementation(this Web web)
#endif
        {
            if (web.HasSitePolicyApplied())
            {
                var expirationDate = ProjectPolicy.GetProjectExpirationDate(web.Context, web);
#if ONPREMISES
                web.Context.ExecuteQueryRetry();
#else
                await web.Context.ExecuteQueryRetryAsync();
#endif
                return expirationDate.Value;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the site closure date
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>DateTime value holding the closure date, DateTime.MinValue in case there was no policy applied</returns>
        public static DateTime GetSiteCloseDate(this Web web)
        {
#if ONPREMISES
            return web.GetSiteCloseDateImplementation();
#else
            return Task.Run(() => web.GetSiteCloseDateImplementation()).GetAwaiter().GetResult();
#endif
        }
#if !ONPREMISES
        /// <summary>
        /// Gets the site closure date
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>DateTime value holding the closure date, DateTime.MinValue in case there was no policy applied</returns>
        public static async Task<DateTime> GetSiteCloseDateAsync(this Web web)
        {
            await new SynchronizationContextRemover();
            return await web.GetSiteCloseDateImplementation();
        }
#endif
        /// <summary>
        /// Gets the site closure date
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>DateTime value holding the closure date, DateTime.MinValue in case there was no policy applied</returns>
#if ONPREMISES
        private static DateTime GetSiteCloseDateImplementation(this Web web)
#else
        private static async Task<DateTime> GetSiteCloseDateImplementation(this Web web)
#endif
        {
            if (web.HasSitePolicyApplied())
            {
                var closeDate = ProjectPolicy.GetProjectCloseDate(web.Context, web);
#if ONPREMISES
                web.Context.ExecuteQueryRetry();
#else
                await web.Context.ExecuteQueryRetryAsync();
#endif
                return closeDate.Value;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets a list of the available site policies
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>A list of <see cref="SitePolicyEntity"/> objects</returns>
        public static List<SitePolicyEntity> GetSitePolicies(this Web web)
        {
            var sitePolicies = ProjectPolicy.GetProjectPolicies(web.Context, web);
            web.Context.Load(sitePolicies);
            web.Context.ExecuteQueryRetry();

            var policies = new List<SitePolicyEntity>();

            if (sitePolicies != null && sitePolicies.Count > 0)
            {
                foreach (var policy in sitePolicies)
                {
                    policies.Add(new SitePolicyEntity
                    {
                        Name = policy.Name,
                        Description = policy.Description,
                        EmailBody = policy.EmailBody,
                        EmailBodyWithTeamMailbox = policy.EmailBodyWithTeamMailbox,
                        EmailSubject = policy.EmailSubject
                    });
                }
            }

            return policies;
        }

        /// <summary>
        /// Gets the site policy that currently is applied
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>A <see cref="SitePolicyEntity"/> object holding the applied policy</returns>
        public static SitePolicyEntity GetAppliedSitePolicy(this Web web)
        {
            if (web.HasSitePolicyApplied())
            {
                var policy = ProjectPolicy.GetCurrentlyAppliedProjectPolicyOnWeb(web.Context, web);
                web.Context.Load(policy,
                             p => p.Name,
                             p => p.Description,
                             p => p.EmailSubject,
                             p => p.EmailBody,
                             p => p.EmailBodyWithTeamMailbox);
                web.Context.ExecuteQueryRetry();
                return new SitePolicyEntity
                {
                    Name = policy.Name,
                    Description = policy.Description,
                    EmailBody = policy.EmailBody,
                    EmailBodyWithTeamMailbox = policy.EmailBodyWithTeamMailbox,
                    EmailSubject = policy.EmailSubject
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the site policy with the given name
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <param name="sitePolicy">Site policy to fetch</param>
        /// <returns>A <see cref="SitePolicyEntity"/> object holding the fetched policy</returns>
        public static SitePolicyEntity GetSitePolicyByName(this Web web, string sitePolicy)
        {
            var policies = web.GetSitePolicies();

            if (policies.Count > 0)
            {
                var policy = policies.FirstOrDefault(p => p.Name == sitePolicy);
                return policy;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Apply a policy to a site
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <param name="sitePolicy">Policy to apply</param>
        /// <returns>True if applied, false otherwise</returns>
        public static bool ApplySitePolicy(this Web web, string sitePolicy)
        {
            var result = false;

            var sitePolicies = ProjectPolicy.GetProjectPolicies(web.Context, web);
            web.Context.Load(sitePolicies);
            web.Context.ExecuteQueryRetry();

            if (sitePolicies != null && sitePolicies.Count > 0)
            {
                var policyToApply = sitePolicies.FirstOrDefault(p => p.Name == sitePolicy);

                if (policyToApply != null)
                {
                    ProjectPolicy.ApplyProjectPolicy(web.Context, web, policyToApply);
                    web.Context.ExecuteQueryRetry();
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Check if a site is closed
        /// </summary>
        /// <param name="web">Web to operate on</param>
        /// <returns>True if site is closed, false otherwise</returns>
        public static bool IsClosedBySitePolicy(this Web web)
        {
            var isClosed = ProjectPolicy.IsProjectClosed(web.Context, web);
            web.Context.ExecuteQueryRetry();
            return isClosed.Value;
        }

        /// <summary>
        /// Close a site, if it has a site policy applied and is currently not closed
        /// </summary>
        /// <param name="web"></param>
        /// <returns>True if site was closed, false otherwise</returns>
        public static bool SetClosedBySitePolicy(this Web web)
        {
            if (web.HasSitePolicyApplied() && !IsClosedBySitePolicy(web))
            {
                ProjectPolicy.CloseProject(web.Context, web);
                web.Context.ExecuteQueryRetry();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Open a site, if it has a site policy applied and is currently closed
        /// </summary>
        /// <param name="web"></param>
        /// <returns>True if site was opened, false otherwise</returns>
        public static bool SetOpenBySitePolicy(this Web web)
        {
            if (web.HasSitePolicyApplied() && IsClosedBySitePolicy(web))
            {
                ProjectPolicy.OpenProject(web.Context, web);
                web.Context.ExecuteQueryRetry();
                return true;
            }
            return false;
        }
    }
}
