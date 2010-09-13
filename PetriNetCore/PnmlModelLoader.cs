using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PetriNetCore
{
    public static class PnmlModelLoader
    {
        private static int seed;

        public static int GetId()
        {
            return ++seed;
        }

        public static IDictionary<string, Marking> LoadMarkings(string path, IEnumerable<GraphPetriNet> nets)
        {
            XNamespace ns = "http://www.example.org/pnml";
            XDocument doc = XDocument.Load(path);
            var netmarkings = from n in doc.Descendants(ns + "net")
                              let netid = n.Attribute("id").Value
                              let net = nets.Where(x => x.Id == netid).First()
                              let placeMarkings = from p in n.Descendants(ns + "place")
                                                  let Id = p.Attribute("id").Value
                                                  let place = net.Places.Where(x => x.Value == Id).Single()
                                                  let Marking =
                                                      int.Parse(
                                                          p.Element(ns + "initialMarking").Element(ns + "text").Value ??
                                                          "0")
                                                  orderby place.Key
                                                  select Tuple.Create(place.Key, Marking)
                              let maxId = placeMarkings.Max(x => x.Item1)
                              select new
                                         {
                                             name = netid,
                                             marking = new Marking(maxId + 1,
                                                                   placeMarkings.ToDictionary(x => x.Item1, x => x.Item2))
                                         };
            return netmarkings.ToDictionary(x => x.name, x => x.marking);
        }

        public static IEnumerable<GraphPetriNet> Load(string path)
        {
            XNamespace ns = "http://www.example.org/pnml";
            XDocument doc = XDocument.Load(path);
            IEnumerable<GraphPetriNet> x = from n in doc.Descendants(ns + "net")
                                           let netid = n.Attribute("id").Value
                                           let places = from p in n.Descendants(ns + "place")
                                                        let Guid = GetId()
                                                        let Id = p.Attribute("id").Value
                                                        let Name = p.Element(ns + "name").Element(ns + "text").Value
                                                        let Marking =
                                                            int.Parse(
                                                                p.Element(ns + "initialMarking").Element(ns + "text").
                                                                    Value ?? "0")
                                                        select new { Guid, Id, Name, Marking }
                                           let transitions = from t in n.Descendants(ns + "transition")
                                                             let Guid = GetId()
                                                             let Id = t.Attribute("id").Value
                                                             select new { Guid, Id }
                                           let inarcs = from t in transitions
                                                        let ia = from a in n.Descendants(ns + "arc")
                                                                 let sourceId = (from p in places
                                                                                 where
                                                                                     p.Id == a.Attribute("source").Value
                                                                                 select p.Guid).SingleOrDefault()
                                                                 where a.Attribute("target").Value == t.Id
                                                                 select new InArc(sourceId)
                                                        select new { t.Guid, inArcs = ia.ToList() }
                                           let outarcs = from t in transitions
                                                         let oa = from a in n.Descendants(ns + "arc")
                                                                  let targetId = (from p in transitions
                                                                                  where
                                                                                      p.Id ==
                                                                                      a.Attribute("target").Value
                                                                                  select p.Guid).SingleOrDefault()
                                                                  where a.Attribute("target").Value == t.Id
                                                                  select new OutArc(targetId)
                                                         select new { t.Guid, outArcs = oa.ToList() }
                                           where n.Attribute("type").Value == "http://www.example.org/pnml/PTNet"
                                           select new GraphPetriNet(
                                               netid,
                                               places.ToDictionary(y => y.Guid, y => y.Id),
                                               transitions.ToDictionary(y => y.Guid, y => y.Id),
                                               inarcs.ToDictionary(y => y.Guid, y => y.inArcs),
                                               outarcs.ToDictionary(y => y.Guid, y => y.outArcs)
                                               );
            return x;
        }
    }

    public class NewPnmlLoader<TModelType> where TModelType : class
    {
        public NewPnmlLoader()
        {
        }

        protected CreatePetriNet Builder { get; set; }

        public IEnumerable<TModelType> Load(string modelPath)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(modelPath));
            if (!File.Exists(modelPath))
            {
                throw new ApplicationException("Model cannot be found");
            }
            XDocument doc = XDocument.Parse(File.ReadAllText(modelPath));
            if (doc == null)
            {
                throw new ApplicationException("Unable to read contents of model file");
            }

            var names = GetModelNames(doc);

            foreach (string name in names)
            {
                var netroot = doc.Descendants("net").Where(element => element.Attribute("id").Value.Equals(name)).Single();
                this.Builder = new CreatePetriNet(name);
                var placeNames = GatherPlaceNames(netroot);
                var transitionNames = GatherTransitionNames(netroot);
                var inArcs = GatherInArcs(netroot, transitionNames);
                var outArcs = GatherOutArcs(netroot, placeNames);
                Builder.WithPlaces(placeNames.Select(tuple => tuple.Item1).ToArray());
                Builder.WithTransitions(transitionNames.Select(tuple => tuple.Item1).ToArray());
                foreach (var inArc in inArcs)
                {
                    var arc = Builder.With(inArc.Item3).FedBy(inArc.Item2).Weight(inArc.Item4);
                    if (inArc.Item5) // is inhibitor
                    {
                        arc.AsInhibitor();
                    }
                    arc.Done();
                }
                foreach (var outArc in outArcs)
                {
                    var arc = Builder.With(outArc.Item2).Feeding(outArc.Item3).Weight(outArc.Item4);
                    arc.Done();
                }
                yield return Builder.CreateNet<TModelType>();
            }
        }

        //(arcname, placename, tranname, weight, inhib)
        IEnumerable<Tuple<string, string, string, int, bool>> GatherInArcs(XElement doc, IEnumerable<Tuple<string, int>> transitions)
        {
            return from x in doc.Descendants("arc")
                   let name = x.Attribute("id").Value
                   let placeName = x.Attribute("source").Value
                   let tranName = x.Attribute("target").Value
                   let weight = x.Descendants("inscription").Descendants("value").SingleOrDefault()
                   let inhibitor = x.Descendants("type").Where(a => a.Attribute("value").Value == "inhibitor").Count() == 1
                   where transitions.Any(tuple => tuple.Item1.Equals(tranName))
                   select Tuple.Create(
                        name, 
                        placeName, 
                        tranName,
                        weight != null ? int.Parse(weight.Value) : 1, 
                        inhibitor);
        }

        //(arcname, tranname, placename, weight)
        IEnumerable<Tuple<string, string, string, int>> GatherOutArcs(XElement doc, IEnumerable<Tuple<string, int>> places)
        {
            return from x in doc.Descendants("arc")
                   let name = x.Attribute("id").Value
                   let placeName = x.Attribute("target").Value
                   let tranName = x.Attribute("source").Value
                   let weight = x.Descendants("inscription").Descendants("value").SingleOrDefault()
                   where places.Any(tuple => tuple.Item1.Equals(tranName))
                   select Tuple.Create(
                        name,
                        placeName,
                        tranName,
                        weight != null ? int.Parse(weight.Value) : 1);
        }

        private IEnumerable<Tuple<string, int>> GatherTransitionNames(XElement doc)
        {
            return from e in doc.Descendants("transition")
                   let priority = e.Descendants("priority").Descendants("value").SingleOrDefault()
                   select Tuple.Create(e.Attribute("id").Value, int.Parse((priority != null) ? priority.Value.Trim() : "1"));
        }

        private IEnumerable<Tuple<string, int>> GatherPlaceNames(XElement doc)
        {
            return from e in doc.Descendants("place")
                   let capacity = e.Descendants("capacity").Descendants("value").SingleOrDefault()
                   select Tuple.Create(e.Attribute("id").Value, int.Parse((capacity != null) ? capacity.Value.Trim() : "0"));
        }

        private IEnumerable<string> GetModelNames(XDocument doc)
        {
            return from n in doc.Descendants("net")
                   where n.Attribute("type").Value == "P/T net"
                   select n.Attribute("id").Value;
        }
    }
}