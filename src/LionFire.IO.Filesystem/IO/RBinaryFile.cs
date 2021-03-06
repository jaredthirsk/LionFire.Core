﻿using LionFire.Persistence;
using LionFire.Resolves;
using MorseCode.ITask;
using System.IO;
using System.Threading.Tasks;

namespace LionFire.IO
{
    public class RBinaryFile : RLocalFileBase<byte[]>
    {

        #region Construction

        public RBinaryFile() { }
        public RBinaryFile(string path) : base(path)
        {
        }

        #endregion

        protected override async ITask<IResolveResult<byte[]>> ResolveImpl()
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(Path))
                {
                    return RetrieveResult<byte[]>.NotFound;
                }

                //return RetrieveResult<byte[]>.Success(OnRetrievedObject(File.ReadAllBytes(Path)));
                return RetrieveResult<byte[]>.Success(File.ReadAllBytes(Path));
            }).ConfigureAwait(false);
        }
    }
}
