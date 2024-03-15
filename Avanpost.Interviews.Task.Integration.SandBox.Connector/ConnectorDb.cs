using Avanpost.Interviews.Task.Integration.Data.Models;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;
using Microsoft.EntityFrameworkCore;
using Orm;
using Orm.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    public class ConnectorDb : IConnector
    {
        private const string permissionTemplate = @"\w+:\d+";

        private UsersDbContext dbContext;

        public ConnectorDb() { }

        public void StartUp(string connectionString)
        {
			DbContextOptionsBuilder<UsersDbContext> optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
            DbContextOptions<UsersDbContext> options;
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = connectionString;
            try
            {
				string conString = builder["ConnectionString"] as string ?? throw new Exception("Unable to find connection string.");
				string providerName = builder["Provider"] as string ?? throw new Exception("Unable to find database provider.");
				if (providerName.Contains("SqlServer"))
				{
					options = optionsBuilder.UseSqlServer(conString).Options;
				}
				else if (providerName.Contains("PostgreSQL"))
				{
					options = optionsBuilder.UseNpgsql(conString).Options;
				}
				else
				{
					throw new Exception("Unsupported database provider");
				}
				dbContext = new UsersDbContext(options);
			}
            catch(Exception ex)
            {
                Logger.Error(ex.Message);
                throw;
            }
        }

        public void CreateUser(UserToCreate user)
        {
            try
            {
				User newUser = new User
				{
					login = user.Login,
					lastName = user.Properties.SingleOrDefault(x => x.Name == nameof(newUser.lastName))?.Value ?? string.Empty,
					firstName = user.Properties.SingleOrDefault(x => x.Name == nameof(newUser.firstName))?.Value ?? string.Empty,
					middleName = user.Properties.SingleOrDefault(x => x.Name == nameof(newUser.middleName))?.Value ?? string.Empty,
					telephoneNumber = user.Properties.SingleOrDefault(x => x.Name == nameof(newUser.telephoneNumber))?.Value ?? string.Empty,
					isLead = Convert.ToBoolean(user.Properties.SingleOrDefault(x => x.Name == nameof(newUser.isLead))?.Value)
				};
				dbContext.Users.Add(newUser);
				Password newPassword = new Password
				{
					userId = user.Login,
					password = user.HashPassword
				};
				dbContext.Passwords.Add(newPassword);
				dbContext.SaveChanges();
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
				throw;
			}
		}

        public IEnumerable<Property> GetAllProperties()
        {
			return new[]
			{
				dbContext.Users.EntityType.GetProperties().Where(x => !x.IsKey()).Select(x => new Property(x.Name, string.Empty)),
				//вынужден писать такие костыли, так как изначально БД спроектировано не совсем правильно
				//нужно, чтобы в таблице Passwords поле "userId" указывалось как внешний ключ, тогда при вытагивании моделей классов
				//у нас в классе User будет навигационное свойство "password", которое будет указывать на объект типа Password
				dbContext.Passwords.EntityType.GetProperties().Where(x => x.Name == "password").Select(x => new Property(x.Name, string.Empty))
			}.SelectMany(x => x);
		}

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
			try
			{
				User user = dbContext.Users.Find(userLogin) ?? throw new Exception("Such user does not exist");
				return dbContext.Entry(user).Properties.Select(x => new UserProperty(x.Metadata.Name, x.CurrentValue?.ToString() ?? string.Empty));
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
				throw;
			}
		}

		public bool IsUserExists(string userLogin)
        {
            if(dbContext.Users.Any(x => x.login == userLogin))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            try
            {
				User user = dbContext.Users.Find(userLogin) ?? throw new Exception("Such user does not exist");
				foreach (UserProperty userProperty in properties)
				{
					//вынужден писать такие костыли, так как изначально БД спроектировано не совсем правильно
					//нужно, чтобы в таблице Passwords поле "userId" указывалось как внешний ключ, тогда при вытагивании моделей классов
					//у нас в классе User будет навигационное свойство "password", которое будет указывать на объект типа Password
					if (userProperty.Name == "password")
					{
						dbContext.Passwords.Single(x => x.userId == userLogin).password = userProperty.Value;
					}
					dbContext.Entry(user).Property(userProperty.Name).CurrentValue = userProperty.Value;	
				}
				dbContext.SaveChanges();
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
				throw;
			}
		}

        public IEnumerable<Permission> GetAllPermissions()
        {
            return new[] { 
                dbContext.RequestRights.Select(x => new Permission(x.id.ToString(), x.name, string.Empty)), 
                dbContext.ItRoles.Select(x => new Permission(x.id.ToString(), x.name, string.Empty)) 
            }.SelectMany(x => x);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            try
            {
				foreach (string permission in rightIds)
				{
					if (!IsRightExpression(permission, permissionTemplate))
					{
						throw new Exception("Permission must be in format \"*Permission type*:*Permision ID*\"");
					}
					string permissionType = permission.Split(':')[0];
					int permissionId = Convert.ToInt32(permission.Split(':')[1]);
					if (permissionType.Equals("Role"))
					{
						dbContext.UserITRoles.Add(new UserITRole { userId = userLogin, roleId = permissionId });
					}
					else if (permissionType.Equals("Request"))
					{
						dbContext.UserRequestRights.Add(new UserRequestRight { userId = userLogin, rightId = permissionId });
					}
					else
					{
						throw new Exception("Unsupported permission type");
					}
				}
				dbContext.SaveChanges();
			}
			catch(Exception ex) 
			{
				Logger.Error(ex.Message);
				throw;
			}
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
			try
			{
				foreach (string permission in rightIds)
				{
					if (!IsRightExpression(permission, permissionTemplate))
					{
						throw new Exception("Permission must be in format \"*Permission type*:*Permision ID*\"");
					}
					string type = permission.Split(':')[0];
					int roleId = Convert.ToInt32(permission.Split(':')[1]);
					if (type.Equals("Role"))
					{
						UserITRole userITRole = dbContext.UserITRoles.First(x => x.userId == userLogin && x.roleId == roleId);
						dbContext.UserITRoles.Remove(userITRole);
					}
					else if (type.Equals("Request"))
					{
						UserRequestRight userRequestRight = dbContext.UserRequestRights.First(x => x.userId == userLogin && x.rightId == roleId);
						dbContext.UserRequestRights.Remove(userRequestRight);
					}
					else
					{
						throw new Exception("Unsupported permission type");
					}
				}
				dbContext.SaveChanges();
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
				throw;
			}
		}

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            var userRoles = dbContext.UserITRoles
                .Where(x => x.userId == userLogin)
                .Join(dbContext.ItRoles, 
                outer => outer.roleId, 
                inner => inner.id, 
                (outer, inner) => inner.name);
            var userRequestRights = dbContext.UserRequestRights
                .Where(x => x.userId == userLogin)
                .Join(dbContext.RequestRights, 
                outer => outer.rightId, 
                inner => inner.id, 
                (outer, inner) => inner.name);
            return new[] {
                userRoles, 
                userRequestRights}
            .SelectMany(x => x);
        }

        public ILogger Logger { get; set; }

        private bool IsRightExpression(string expression, string template)
        {
            Regex regex = new Regex(template);
            return regex.IsMatch(expression);
        }
    }
}