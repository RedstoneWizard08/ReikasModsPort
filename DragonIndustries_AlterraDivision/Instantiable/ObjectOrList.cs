using System;
using System.Collections.Generic;
using System.Linq;

namespace ReikaKalseki.DIAlterra;

/// <summary>
/// Supply 0-N args, where 0 is interpreted as null, 1 as a single, and 2+ as a collection
/// </summary>
public sealed class ObjectOrList<E> {

	public readonly bool isCollection;
	private readonly List<E> objects = [];

	public ObjectOrList(params E[] obj) {
		objects.AddRange(obj.AsEnumerable());
		isCollection = objects.Count > 1;
	}

	public E value => isCollection ? throw new NotImplementedException("Object is a collection!") : objects.Count == 0 ? default(E) : objects[0];

	public IReadOnlyCollection<E> values => isCollection ? (IReadOnlyCollection<E>)objects.AsReadOnly() : throw new NotImplementedException("Object is not a collection!");

	public override string ToString() {
		if (isCollection)
			return objects.toDebugString();
		var obj = value;
		return obj == null ? "null" : obj.ToString();
	}

}