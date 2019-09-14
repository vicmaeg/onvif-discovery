using System;
using System.Collections.Generic;
using System.Linq;

namespace OnvifSharp.Discovery.Models
{
	public class DiscoveryDevice : IEquatable<DiscoveryDevice>
	{
		public IEnumerable<string> Types { get; internal set; }
		public IEnumerable<string> XAdresses { get; internal set; }
		public string Model { get; internal set; }
		public string Mfr { get; internal set; }
		public string Address { get; internal set; }

		public bool Equals (DiscoveryDevice other)
		{
			if (ReferenceEquals (null, other)) {
				return false;
			}
			if (other.Model != Model || other.Mfr != Mfr || other.Address != Address
				|| other.Types.Count () != Types.Count () || other.XAdresses.Count () != XAdresses.Count ()) {
				return false;
			}
			for (var i = 0; i < Types.Count (); i++) {
				if (!Types.ElementAt (i).Equals (other.Types.ElementAt (i))) {
					return false;
				}
			}
			for (var i = 0; i < XAdresses.Count (); i++) {
				if (!XAdresses.ElementAt (i).Equals (other.XAdresses.ElementAt (i))) {
					return false;
				}
			}
			return true;
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as DiscoveryDevice);
		}

		public override int GetHashCode ()
		{
			unchecked {
				// Choose large primes to avoid hashing collisions
				const int HashingBase = (int)2166136261;
				const int HashingMultiplier = 16777619;

				int hash = HashingBase;
				if (Types != null) {
					foreach (var type in Types) {
						hash = (hash * HashingMultiplier) ^ type.GetHashCode ();
					}
				}
				if (XAdresses != null) {
					foreach (var address in XAdresses) {
						hash = (hash * HashingMultiplier) ^ address.GetHashCode ();
					}
				}
				hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals (null, Model) ? Model.GetHashCode () : 0);
				hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals (null, Mfr) ? Mfr.GetHashCode () : 0);
				hash = (hash * HashingMultiplier) ^ (!Object.ReferenceEquals (null, Address) ? Address.GetHashCode () : 0);
				return hash;
			}
		}

		public static bool operator == (DiscoveryDevice devA, DiscoveryDevice devB)
		{
			if (Object.ReferenceEquals (devA, devB)) {
				return true;
			}

			if (Object.ReferenceEquals (null, devA)) {
				return false;
			}

			return (devA.Equals (devB));
		}

		public static bool operator != (DiscoveryDevice devA, DiscoveryDevice devB)
		{
			return !(devA == devB);
		}
	}
}
