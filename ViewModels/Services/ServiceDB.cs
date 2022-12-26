using Models.DTO;
using Models.Entities;
using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace ViewModels.Services
{
    public static class ServiceDB 
    {
        public static int AddOrUpdate<T>(T entity) where T : class, IEntityWithId
        {
            try
            {
                using (BoosterEntities db = new BoosterEntities())
                {

                    var entityList = db.Set<T>();

                    var origin = entityList.SingleOrDefault(p => p.ID.ToString() == entity.ID.ToString());
                    if (origin != null)
                    {

                        db.Entry(origin).CurrentValues.SetValues(entity);
                    }
                    else
                    {
                        entity.ID = Guid.NewGuid();
                        entityList.Add(entity);
                    }
                    return db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }
        public static int Delete<T>(T entity) where T : class, IEntityWithId
        {
            try
            {
                using (BoosterEntities dbContext = new BoosterEntities())
                {
                    dbContext.Entry(entity).State = EntityState.Deleted;
                    return dbContext.SaveChanges();
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {


                MessageBox.Show(ex.Message);
                return 0;
            }
        }
        public static ObservableCollection<T> UpdateUI<T>(ObservableCollection<T> entityList) where T : class
        {
            using (BoosterEntities dbContext = new BoosterEntities())
            {
                var list = dbContext.Set<T>();
                entityList.Clear();
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        var dto = item;
                        entityList.Add(dto);
                    }
                }
               
                return entityList;
            }
        }
        public static List<T> GetAll<T>() where T : class
        {

            try
            {
                using (BoosterEntities dbContext = new BoosterEntities())
                {
                    List<T> collection = dbContext.Set<T>().ToList();
                    return collection;
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
#if (DEBUG)

                throw;
#else
                MessageBox.Show(ex.Message);
                return null;
#endif
            }
        }
        public static T FindByID<T>(Guid ID) where T : class, IEntityWithId
        {

            try
            {
                using (BoosterEntities dbContext = new BoosterEntities())
                {
                    return dbContext.Set<T>().SingleOrDefault(t => t.ID == ID);
                }
            }
#pragma warning disable 0168
            catch (Exception ex)
#pragma warning restore 0168
            {
#if (DEBUG)

                throw;
#else
                MessageBox.Show(ex.Message);
                return null;
#endif
            }
        }
        public static void GetAllUsers()
        {
            using (BoosterEntities db = new BoosterEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                DB.Users = new ObservableCollection<User>(db.GetUser());
            }
        }
        public static void GetOtherStuff()
        {
            using (BoosterEntities db = new BoosterEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                DB.Logs = new ObservableCollection<Log>(db.GetLogs());
                DB.Amplifiers = new ObservableCollection<Amplifier>(db.GetAmplifiers());
                DB.Settings = new ObservableCollection<GeneralSetting>(db.GetSettings());
            }
        }

    }
}
