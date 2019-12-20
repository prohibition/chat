using Chat.Web.Models.ViewModels;
using Chat.Web.TCModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Helpers
{
    public class TitleConnectHelper
    {
        public static User GetUserbyCompanyEmail(string companyEmail)
        {
            using (var context=new TitleConnectGroupEntities())
            {
                return context.Users.FirstOrDefault(x => x.CompanyEmail == companyEmail);
            }
        }

        public static List<User> GetUserbyCompanyEmails(string[] companyEmails)
        {
            using (var context = new TitleConnectGroupEntities())
            {
                return context.Users.Where(x => companyEmails.Contains(x.CompanyEmail)).ToList();
            }
        }



        public static User GetUserbyToken(string token)
        {
            using (var context = new TitleConnectGroupEntities())
            {
                return context.Users.FirstOrDefault(x => x.UserToken == token);
            }
        }


        public static List<CompanyViewModel> GetCompanies()
        {
            using (var context = new TitleConnectGroupEntities())
            {
                return (
                    from c in context.Companies where c.isDeleted==false

                    select new CompanyViewModel
                    {
                        CompanyId = c.CompanyId,

                        CompanyName = c.CompanyName

                    }).ToList().OrderBy(x => x.CompanyName).ToList();


            }
        }

        public static List<User> GetAllUsers()
        {
            using (var context = new TitleConnectGroupEntities())
            {
                var companies = context.Companies.ToList();

                var userlist= (
                    from c in context.Companies
                    join u in context.Users  on c.CompanyId equals u.CompanyId
                    where u.isActive == true && u.isApproved==true select u
                    ).ToList().OrderBy(x=>x.DisplayName).ToList();

                 userlist.ForEach(x => { x.CompanyName = companies.FirstOrDefault(c => c.CompanyId == x.CompanyId).CompanyName; });

                return userlist;
            }
        }

        public static List<User> GetCompanyUsers(int companyId)
        {
            using (var context = new TitleConnectGroupEntities())
            {
                var companies = context.Companies.ToList();

                var userlist = (
                    from c in context.Companies
                    join u in context.Users on c.CompanyId equals u.CompanyId
                    where u.isActive == true && u.isApproved == true && c.CompanyId==companyId && u.isDeleted==false 
                    //&& (u.isGhostUser==false || u.isGhostUser==null) 
                    && u.UserType !=1
                    select u
                    ).ToList().OrderBy(x => x.DisplayName).ToList();

                userlist.ForEach(x => { x.CompanyName = companies.FirstOrDefault(c => c.CompanyId == x.CompanyId).CompanyName; });

                return userlist;
            }
        }

        public static bool CheckUser(string companyEmail)
        {
            using (var context = new TitleConnectGroupEntities())
            {
                return context.Users.Where(x => x.isActive == true && x.isApproved==true && x.CompanyEmail == companyEmail).Any();
            }
        }
    }
}