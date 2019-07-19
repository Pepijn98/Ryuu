using System.Collections.Generic;
using Ryuu.Models;

// ReSharper disable UnusedMemberInSuper.Global

namespace Ryuu.Interfaces
{
    public interface ICatgirl
    {
        List<CatgirlIdModel> Images { get; set; }
    }
    
    public interface ICatgirlId
    {
        string Id { get; set; }
    }
}