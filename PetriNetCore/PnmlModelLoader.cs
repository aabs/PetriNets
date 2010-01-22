using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PetriNetCore
{
    public class ArcDto
    {
        public int Guid{get;set;}
        public string Id{get;set;}
        public int FromId{get;set;}
        public int ToId{get;set;}
        public bool FromPlace { get; set; }
    }
    public static class PnmlModelLoader
    {
        static int seed = 0;
        public static int GetId() { return ++seed; }
        public static IEnumerable<OrdinaryPetriNet> Load(string path)
        {
            XNamespace ns = "http://www.example.org/pnml";
            var doc = XDocument.Load(path);
            var x = from n in doc.Descendants(ns + "net")
                    let netid = n.Attribute("id").Value
                    let places = from p in n.Descendants(ns + "place")
                                 let Guid = GetId()
                                 let Id = p.Attribute("id").Value
                                 let Name = p.Element(ns + "name").Element(ns + "text").Value
                                 let Marking = int.Parse(p.Element(ns + "initialMarking").Element(ns + "text").Value??"0")
                                 select new { Guid, Id, Name, Marking }
                    let transitions = from t in n.Descendants(ns + "transition")
                                      let Guid = GetId()
                                      let Id = t.Attribute("id").Value
                                      select new { Guid, Id }
                    let arcs = from a in n.Descendants(ns + "arc")
                               let Guid = GetId()
                               let Id = a.Attribute("id").Value
                               let Source = a.Attribute("source").Value
                               let Target = a.Attribute("target").Value
                               let FromPlace = (from p in places where p.Id == Source select p).Count() > 0
                               let FromId = (FromPlace) ?
                                                (from p in places where p.Id == Source select p.Guid).Single() :
                                                (from p in transitions where p.Id == Source select p.Guid).Single()
                               let ToId = (!FromPlace) ?
                                                (from p in places where p.Id == Target select p.Guid).Single() :
                                                (from p in transitions where p.Id == Target select p.Guid).Single()
                               select new ArcDto
                               {
                                   Guid = Guid,
                                   Id = Id,
                                   FromId = FromId,
                                   ToId = ToId,
                                   FromPlace = FromPlace
                               }
                    where n.Attribute("type").Value == "http://www.example.org/pnml/PTNet"
                    select new OrdinaryPetriNet(
                        netid,
                        places.Select(p => Tuple.Create<int, string,int>(p.Guid, p.Id, p.Marking)).ToList(),
                        transitions.Select(t => Tuple.Create<int, string>(t.Guid, t.Id)).ToList(),
                        arcs.ToList()
                        );
            return x;
        }
    }
}
