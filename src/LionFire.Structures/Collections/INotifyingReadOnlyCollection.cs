﻿using System.Collections.Generic;

namespace LionFire.Collections
{
    //    public interface INotifyListChanged<ChildType> FUTURE - for efficient list updates in WPF?
    //{
    //    event NotifyListChangedHandler<ChildType> CollectionChanged;
    //}

    public interface INotifyingReadOnlyCollection<out T> : IReadOnlyCollection<T>, INotifyCollectionChanged<T>
    {
    }

}
