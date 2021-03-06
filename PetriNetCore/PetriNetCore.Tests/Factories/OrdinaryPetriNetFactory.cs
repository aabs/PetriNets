// <copyright file="OrdinaryPetriNetFactory.cs" company="Microsoft">Copyright © Microsoft 2009</copyright>

using System;
using Microsoft.Pex.Framework;
using PetriNetCore;
using System.Collections.Generic;

namespace PetriNetCore
{
    /// <summary>A factory for PetriNetCore.OrdinaryPetriNet instances</summary>
    public static partial class OrdinaryPetriNetFactory
    {
        /// <summary>A factory for PetriNetCore.OrdinaryPetriNet instances</summary>
        [PexFactoryMethod(typeof(OrdinaryPetriNet))]
        public static OrdinaryPetriNet Create(
        )
        {
            OrdinaryPetriNet ordinaryPetriNet
               = new OrdinaryPetriNet(
                "p",
                new Dictionary<int, string> {
                    {0, "p0"},
                    {1, "p1"},
                    {2, "p2"}
                },
                new Dictionary<int, int> { { 0, 1 }, { 1, 1 } },
                new Dictionary<int, string> { { 0, "t0" } },
                new Dictionary<int, List<InArc>>(){
                    {0, new List<InArc>(){new InArc(0),new InArc(1)}}
                },
                new Dictionary<int, List<OutArc>>(){
                    {0, new List<OutArc>(){new OutArc(2)}}
                }
              );
            return ordinaryPetriNet;
        }
    }
}
