// Dafny program Filter.dfy compiled into C#
// To recompile, you will need the libraries
//     System.Runtime.Numerics.dll System.Collections.Immutable.dll
// but the 'dotnet' tool in .NET should pick those up automatically.
// Optionally, you may want to include compiler switches like
//     /debug /nowarn:162,164,168,183,219,436,1717,1718

using System;
using System.Numerics;
using System.Collections;
[assembly: DafnyAssembly.DafnySourceAttribute(@"// dafny 4.11.0.0
// Command-line arguments: translate cs src/Bison.Razor/Verified/Filter.dfy
// Filter.dfy


module {:extern ""Bison.Razor.Models""} Models {
  class {:extern} Taxon {
    function {:extern} isSubTaxon(ancestor: Taxon): bool
      decreases ancestor
  }

  class {:extern} Observation {
    function {:extern} getTaxon(): Taxon?
  }
}

module Filter {
  predicate Matches(root: Taxon, o: Observation)
    decreases root, o
  {
    var t: Taxon? := o.getTaxon();
    t != null &&
    (t == root || t.isSubTaxon(root))
  }

  function FilterBy(root: Taxon, obs: seq<Observation>): (res: seq<Observation>)
    ensures forall o: Observation {:trigger Matches(root, o)} {:trigger o in obs} {:trigger o in res} :: (o in res ==> o in obs) && (o in res ==> Matches(root, o))
    ensures forall o: Observation {:trigger o in res} {:trigger Matches(root, o)} {:trigger o in obs} :: o in obs && Matches(root, o) ==> o in res
    decreases root, obs
  {
    if |obs| == 0 then
      []
    else if Matches(root, obs[0]) then
      [obs[0]] + FilterBy(root, obs[1..])
    else
      FilterBy(root, obs[1..])
  }

  import opened Models
}

module {:extern ""Bison.Razor.Verified""} Wrapper {

  import opened Models

  import Filter
  class TaxonFilter {
    static method FilterBy(root: Taxon, obs: array<Observation>) returns (res: array<Observation>)
      ensures fresh(res)
      ensures res[..] == Filter.FilterBy(root, obs[..])
      decreases root, obs
    {
      var filtered := Filter.FilterBy(root, obs[..]);
      res := new Observation[|filtered|] ((i: int) requires 0 <= i < |filtered| => filtered[i]);
    }
  }
}
")]

namespace Dafny {
  internal class ArrayHelpers {
    public static T[] InitNewArray1<T>(T z, BigInteger size0) {
      int s0 = (int)size0;
      T[] a = new T[s0];
      for (int i0 = 0; i0 < s0; i0++) {
        a[i0] = z;
      }
      return a;
    }
  }
} // end of namespace Dafny
internal static class FuncExtensions {
  public static Func<UResult> DowncastClone<TResult, UResult>(this Func<TResult> F, Func<TResult, UResult> ResConv) {
    return () => ResConv(F());
  }
  public static Func<U, UResult> DowncastClone<T, TResult, U, UResult>(this Func<T, TResult> F, Func<U, T> ArgConv, Func<TResult, UResult> ResConv) {
    return arg => ResConv(F(ArgConv(arg)));
  }
  public static Func<U1, U2, UResult> DowncastClone<T1, T2, TResult, U1, U2, UResult>(this Func<T1, T2, TResult> F, Func<U1, T1> ArgConv1, Func<U2, T2> ArgConv2, Func<TResult, UResult> ResConv) {
    return (arg1, arg2) => ResConv(F(ArgConv1(arg1), ArgConv2(arg2)));
  }
}
// end of class FuncExtensions
namespace Bison.Razor.Models {



} // end of namespace Bison.Razor.Models
namespace Filter {

  public partial class __default {
    public static bool Matches(Bison.Razor.Models.Taxon root, Bison.Razor.Models.Observation o)
    {
      Bison.Razor.Models.Taxon _0_t = (o).getTaxon();
      return ((_0_t) != (object) ((Bison.Razor.Models.Taxon)null)) && (((_0_t) == (object) (root)) || ((_0_t).isSubTaxon(root)));
    }
    public static Dafny.ISequence<Bison.Razor.Models.Observation> FilterBy(Bison.Razor.Models.Taxon root, Dafny.ISequence<Bison.Razor.Models.Observation> obs)
    {
      Dafny.ISequence<Bison.Razor.Models.Observation> _0___accumulator = Dafny.Sequence<Bison.Razor.Models.Observation>.FromElements();
    TAIL_CALL_START: ;
      if ((new BigInteger((obs).Count)).Sign == 0) {
        return Dafny.Sequence<Bison.Razor.Models.Observation>.Concat(_0___accumulator, Dafny.Sequence<Bison.Razor.Models.Observation>.FromElements());
      } else if (Filter.__default.Matches(root, (obs).Select(BigInteger.Zero))) {
        _0___accumulator = Dafny.Sequence<Bison.Razor.Models.Observation>.Concat(_0___accumulator, Dafny.Sequence<Bison.Razor.Models.Observation>.FromElements((obs).Select(BigInteger.Zero)));
        Bison.Razor.Models.Taxon _in0 = root;
        Dafny.ISequence<Bison.Razor.Models.Observation> _in1 = (obs).Drop(BigInteger.One);
        root = _in0;
        obs = _in1;
        goto TAIL_CALL_START;
      } else {
        Bison.Razor.Models.Taxon _in2 = root;
        Dafny.ISequence<Bison.Razor.Models.Observation> _in3 = (obs).Drop(BigInteger.One);
        root = _in2;
        obs = _in3;
        goto TAIL_CALL_START;
      }
    }
  }
} // end of namespace Filter
namespace Bison.Razor.Verified {


  public partial class TaxonFilter {
    public TaxonFilter() {
    }
    public static Bison.Razor.Models.Observation[] FilterBy(Bison.Razor.Models.Taxon root, Bison.Razor.Models.Observation[] obs)
    {
      Bison.Razor.Models.Observation[] res = new Bison.Razor.Models.Observation[0];
      Dafny.ISequence<Bison.Razor.Models.Observation> _0_filtered;
      _0_filtered = Filter.__default.FilterBy(root, Dafny.Helpers.SeqFromArray(obs));
      Func<BigInteger, Bison.Razor.Models.Observation> _init0 = Dafny.Helpers.Id<Func<Dafny.ISequence<Bison.Razor.Models.Observation>, Func<BigInteger, Bison.Razor.Models.Observation>>>((_1_filtered) => ((System.Func<BigInteger, Bison.Razor.Models.Observation>)((_2_i) => {
        return (_1_filtered).Select(_2_i);
      })))(_0_filtered);
      Bison.Razor.Models.Observation[] _nw0 = new Bison.Razor.Models.Observation[Dafny.Helpers.ToIntChecked(new BigInteger((_0_filtered).Count), "array size exceeds memory limit")];
      for (var _i0_0 = 0; _i0_0 < new BigInteger(_nw0.Length); _i0_0++) {
        _nw0[(int)(_i0_0)] = _init0(_i0_0);
      }
      res = _nw0;
      return res;
    }
  }
} // end of namespace Bison.Razor.Verified
namespace _module {

} // end of namespace _module
