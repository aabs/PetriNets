using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PetriNetCore
{
    public class ArcDto
    {
        public int Guid { get; set; }
        public string Id { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public bool FromPlace { get; set; }
    }
    public static class PnmlModelLoader
    {
        static int seed = 0;
        public static int GetId() { return ++seed; }
        
        public static IDictionary<string, Marking> LoadMarkings(string path, IEnumerable<GraphPetriNet> nets)
        {
            XNamespace ns = "http://www.example.org/pnml";
            var doc = XDocument.Load(path);
            var netmarkings = from n in doc.Descendants(ns + "net")
                              let netid = n.Attribute("id").Value
                              let net = nets.Where(x => x.Id == netid).First()
                              let placeMarkings = from p in n.Descendants(ns + "place")
                                                  let Id = p.Attribute("id").Value
                                                  let place = net.Places.Where(x => x.Value == Id).Single()
                                                  let Marking = int.Parse(p.Element(ns + "initialMarking").Element(ns + "text").Value ?? "0")
                                                  orderby place.Key
                                                  select Tuple.Create(place.Key, Marking)
                              let maxId = placeMarkings.Max(x => x.Item1)
                              select new
                              {
                                  name = netid,
                                  marking = new Marking(maxId+1,
                                    placeMarkings.ToDictionary(x => x.Item1, x => x.Item2))
                              };
            return netmarkings.ToDictionary(x => x.name, x => x.marking);
        }

        public static IEnumerable<GraphPetriNet> Load(string path)
        {
            XNamespace ns = "http://www.example.org/pnml";
            var doc = XDocument.Load(path);
            var x = from n in doc.Descendants(ns + "net")
                    let netid = n.Attribute("id").Value
                    let places = from p in n.Descendants(ns + "place")
                                 let Guid = GetId()
                                 let Id = p.Attribute("id").Value
                                 let Name = p.Element(ns + "name").Element(ns + "text").Value
                                 let Marking = int.Parse(p.Element(ns + "initialMarking").Element(ns + "text").Value ?? "0")
                                 select new { Guid, Id, Name, Marking }
                    let transitions = from t in n.Descendants(ns + "transition")
                                      let Guid = GetId()
                                      let Id = t.Attribute("id").Value
                                      select new { Guid, Id }
                    let inarcs = from t in transitions
                                 let ia = from a in n.Descendants(ns + "arc")
                                          let sourceId = (from p in places
                                                          where p.Id == a.Attribute("source").Value
                                                          select p.Guid).SingleOrDefault()
                                          where a.Attribute("target").Value == t.Id
                                          select new InArc(sourceId)
                                 select new { t.Guid, inArcs = ia.ToList() }
                    let outarcs = from t in transitions
                                  let oa = from a in n.Descendants(ns + "arc")
                                           let targetId = (from p in transitions
                                                           where p.Id == a.Attribute("target").Value
                                                           select p.Guid).SingleOrDefault()
                                           where a.Attribute("target").Value == t.Id
                                           select new OutArc(targetId)
                                  select new { t.Guid, outArcs = oa.ToList() }
                    where n.Attribute("type").Value == "http://www.example.org/pnml/PTNet"
                    select new GraphPetriNet(
                        netid,
                        places.ToDictionary(y => y.Guid, y => y.Id),
//                        places.ToDictionary(y => y.Guid, y => y.Marking),
                        transitions.ToDictionary(y => y.Guid, y => y.Id),
                        inarcs.ToDictionary(y => y.Guid, y => y.inArcs),
                        outarcs.ToDictionary(y => y.Guid, y => y.outArcs)
                        );
            return x;
        }
    }
}

