﻿using System;

namespace RhuEngine.WorldObjects
{
	public class DefaultAttribute : Attribute
	{
		public object Data { get; private set; }
		public DefaultAttribute(object value) {
			Data = value;
		}
	}
}
