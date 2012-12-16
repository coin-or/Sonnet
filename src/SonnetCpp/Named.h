// Copyright (C) 2011, Jan-Willem Goossens 
// All Rights Reserved.
// This code is licensed under the terms of the Eclipse Public License (EPL).

#ifndef Named_H
#define Named_H

#include <string> 

using namespace std;

namespace Sonnet
{
	/// <summary>
	/// The class Named is a base class for entities that have a Name and 
	/// are Compared using an integer ID. 
	/// The ID must be set within the derived constructor.
	/// </summary>
	class Named 
	{
	public:
		/// <summary>
		/// Constructor of a new Named object with the given name.
		/// Empty string is used if no name is provided.
		/// </summary>
		/// <param name="name">The name is the new object, or empty string is not provided.</param>
		Named(const string& name = "")
		{
			setName(name);
		}

		string getTypeName()
		{
			return typeid(*this).name();
		}

		/// <summary>
		/// Gets or sets the name of this object.
		/// Name must be not-null.
		/// </summary>
		string getName()
		{
			return name;
		}

		void setName(const string& value)
		{
			//Ensure.NotNull(value, "name");
			this->name = value;
		}
		/// <summary>
		/// Returns the ID of this object.
		/// </summary>
		int getID()
		{
			return id;
		}

		/// <summary>
		/// Compares this object to the given object and returns true iff the objects are of the same type, and have the same ID.
		/// </summary>
		/// <param name="obj">The object to compare this to.</param>
		/// <returns>True iff the objects are of the same type, and have the same ID.</returns>
		bool Equals(Named& obj)
		{
			return Equals(&obj);
		}

		/// <summary>
		/// Compares this object to the given object and returns true iff the objects are of the same type, and have the same ID.
		/// </summary>
		/// <param name="obj">The object to compare this to.</param>
		/// <returns>True iff the objects are of the same type, and have the same ID.</returns>
		bool Equals(Named* obj)
		{
			return id == obj->id && typeid(*this) == typeid(*obj);
		}

	protected:
		int id;

	private:
		string name;

	};
}

#endif
