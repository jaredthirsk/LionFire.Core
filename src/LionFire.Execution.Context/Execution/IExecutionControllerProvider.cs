﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Execution
{
    public interface IExecutionControllerProvider
    {
        bool TryAttachController(ExecutionContext context);
    }
}
