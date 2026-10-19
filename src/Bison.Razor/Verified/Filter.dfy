//Kør det her dotnet add src/Bison.Razor package DafnyRuntime --version 4.11.0
//Run this if you make changes to this file: dafny translate cs src/Bison.Razor/Verified/Filter.dfy


module {:extern "Bison.Razor.Models"} Models { //tells dafny about our C# classes
  class {:extern} Taxon {
    function {:extern} isSubTaxon(ancestor: Taxon): bool //Tells dafny that a function isSubTaxon exists
  }
  class {:extern} Observation {
    function {:extern} getTaxon(): Taxon
  }
}

module Filter {
  import opened Models //Imports the models

  predicate Matches(root: Taxon, o: Observation) {
    var t := o.getTaxon();
    t != null && (t == root || t.isSubTaxon(root)) //observations without a taxon never match
  }

  function FilterBy(root: Taxon, obs: seq<Observation>): (res: seq<Observation>)
    ensures forall o :: o in res ==> o in obs && Matches(root, o)
    ensures forall o :: o in obs && Matches(root, o) ==> o in res
  {
    if |obs| == 0 then []
    else if Matches(root, obs[0]) then [obs[0]] + FilterBy(root, obs[1..])
    else FilterBy(root, obs[1..])
  }
}

module {:extern "Bison.Razor.Verified"} Wrapper { //The generated C# goes into the namespace "Bison.Razor.Verified"
  import opened Models
  import Filter
  class TaxonFilter {
    static method FilterBy(root: Taxon, obs: array<Observation>) returns (res: array<Observation>)
      ensures fresh(res)
      ensures res[..] == Filter.FilterBy(root, obs[..])
    {
      var filtered := Filter.FilterBy(root, obs[..]);
      res := new Observation[|filtered|](i requires 0 <= i < |filtered| => filtered[i]);
    }
  }
}