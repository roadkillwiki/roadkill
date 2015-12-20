using System;
using System.Collections.Generic;
using System.Linq;
using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Linq;
using Mindscape.LightSpeed.Querying;

namespace Roadkill.Core.Database.LightSpeed
{
	public class LightSpeedUserRepository : IUserRepository
	{
		internal readonly IUnitOfWork _unitOfWork;
		internal IQueryable<UserEntity> Users => UnitOfWork.Query<UserEntity>();

		public IUnitOfWork UnitOfWork
		{
			get
			{
				if (_unitOfWork == null)
				{
                    throw new DatabaseException("The IUnitOfWork for Lightspeed is null", null);
				}

				return _unitOfWork;
			}
		}

		public LightSpeedUserRepository(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void DeleteUser(User user)
		{
			UserEntity entity = UnitOfWork.FindById<UserEntity>(user.Id);
			UnitOfWork.Remove(entity);
			UnitOfWork.SaveChanges();
		}

		public void DeleteAllUsers()
		{
			UnitOfWork.Remove(new Query(typeof(UserEntity)));
			UnitOfWork.SaveChanges();
		}

		public User GetAdminById(Guid id)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Id == id && x.IsAdmin);
			return FromEntity.ToUser(entity);
		}

		public User GetUserByActivationKey(string key)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.ActivationKey == key && x.IsActivated == false);
			return FromEntity.ToUser(entity);
		}

		public User GetEditorById(Guid id)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Id == id && x.IsEditor);
			return FromEntity.ToUser(entity);
		}

		public User GetUserByEmail(string email, bool? isActivated = null)
		{
			UserEntity entity;

			if (isActivated.HasValue)
				entity = Users.FirstOrDefault(x => x.Email == email && x.IsActivated == isActivated);
			else
				entity = Users.FirstOrDefault(x => x.Email == email);

			return FromEntity.ToUser(entity);
		}

		public User GetUserById(Guid id, bool? isActivated = null)
		{
			UserEntity entity;

			if (isActivated.HasValue)
				entity = Users.FirstOrDefault(x => x.Id == id && x.IsActivated == isActivated);
			else
				entity = Users.FirstOrDefault(x => x.Id == id);

			return FromEntity.ToUser(entity);
		}

		public User GetUserByPasswordResetKey(string key)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.PasswordResetKey == key);
			return FromEntity.ToUser(entity);
		}

		public User GetUserByUsername(string username)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Username == username);
			return FromEntity.ToUser(entity);
		}

		public User GetUserByUsernameOrEmail(string username, string email)
		{
			UserEntity entity = Users.FirstOrDefault(x => x.Username == username || x.Email == email);
			return FromEntity.ToUser(entity);
		}

		public IEnumerable<User> FindAllEditors()
		{
			List<UserEntity> entities = Users.Where(x => x.IsEditor).ToList();
			return FromEntity.ToUserList(entities);
		}

		public IEnumerable<User> FindAllAdmins()
		{
			List<UserEntity> entities = Users.Where(x => x.IsAdmin).ToList();
			return FromEntity.ToUserList(entities);
		}

		public User SaveOrUpdateUser(User user)
		{
			UserEntity entity = UnitOfWork.FindById<UserEntity>(user.Id);
			if (entity == null)
			{
				// Turn the domain object into a database entity
				entity = new UserEntity();
				ToEntity.FromUser(user, entity);
				UnitOfWork.Add(entity);
				UnitOfWork.SaveChanges();

				user = FromEntity.ToUser(entity);
			}
			else
			{
				ToEntity.FromUser(user, entity);
				UnitOfWork.SaveChanges();
			}

			return user;
		}

		public void Dispose()
		{
			_unitOfWork.SaveChanges();
			_unitOfWork.Dispose();
		}
	}
}
