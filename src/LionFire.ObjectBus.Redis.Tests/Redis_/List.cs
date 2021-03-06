﻿using System;
using System.Collections.Generic;
using System.Text;
using LionFire.Applications.Hosting;
using LionFire.Hosting;
using LionFire.ObjectBus;
using LionFire.ObjectBus.Redis;
using LionFire.Referencing;
using Xunit;
using LionFire.Persistence.Handles;

namespace Redis_
{
    public class List
    {
        [Fact]
        public async void Pass()
        {
            const string testData = "testData-";

            const int itemCount = 200;

            await FrameworkHostBuilder.Create()
                    .AddObjectBus<RedisOBus>()
                    .Run(async () =>
                    {
                        var path = @"\temp\tests\" + GetType().FullName + @"\" + nameof(Pass) + @"\TestFile";

                        var r = new RedisReference(path);

                        #region Cleanup

                        for (int i = 0; i < itemCount; i++)
                        {
                            var childPath = LionPath.Combine(path, "item-" + i);
                            {
                                var childRef = new RedisReference(childPath);
                                var h = childRef.ToHandle<string>();
                                await h.Delete();
                                Assert.False(await h.Exists(), "test object exists after delete: " + childPath);
                            }
                        }

                        #endregion
                        
                        #region Create

                        for (int i = 0; i < itemCount; i++)
                        {
                            var childPath = LionPath.Combine(path, "item-" + i);
                            {
                                var childRef = new RedisReference(childPath);
                                var h = childRef.ToHandle<string>();
                                Assert.False(await h.Exists(), "test object already exists: " + childPath);
                                h.Value = testData + i;
                                await h.Put();
                                Assert.True(await h.Exists(), "test object does not exist after saving: " + childPath);
                            }
                        }

                        #endregion

                        #region Prep Expected
                        
                        var expected = new HashSet<string>();

                        for (int i = 0; i < itemCount; i++)
                        {
                            expected.Add("item-" + i);
                        }

                        List<string> extras = new List<string>();

                        #endregion

                        #region Get keys

                        var keys = await r.GetCollectionHandle().GetKeys(r); //TODO - extension method
                        foreach(var foundItem in keys)
                        {
                            if(!expected.Remove(foundItem))
                            {
                                extras.Add(foundItem);
                            }
                        }

                        #endregion

                        Assert.Empty(expected);
                        Assert.Empty(extras);

                        #region Cleanup

                        for (int i = 0; i < itemCount; i++)
                        {
                            var childPath = LionPath.Combine(path, "item-" + i);
                            {
                                var childRef = new RedisReference(childPath);
                                var h = childRef.ToHandle<string>();
                                await h.Delete();
                                Assert.False(await h.Exists(), "test object exists after delete: " + childPath);
                            }
                        }

                        #endregion

                        //{
                        //    var h = r.ToHandle<string>();
                        //    Assert.True(await h.Exists(), "test object does not exist after saving: " + path);

                        //    var retrievedData = h.Object;
                        //    Assert.Equal(testData, retrievedData);

                        //    h.MarkDeleted(); // TODO: make one
                        //    await h.Put();
                        //    Assert.False(await h.Exists(), "test object still exists after deleting: " + path);
                        //}
                        //{
                        //    var h = r.ToHandle<string>();
                        //    Assert.False(await h.Exists(), "test object still exists after deleting: " + path);

                        //}
                    });
        }
    }
}
