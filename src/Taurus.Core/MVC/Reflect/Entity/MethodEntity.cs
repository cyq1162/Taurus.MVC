using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Taurus.Mvc
{ 
    internal class MethodEntity
    {
		public MethodInfo Method { get; set; }
		public AttributeEntity AttrEntity { get; set; }
		public ParameterInfo[] Parameters;
		public MethodEntity(MethodInfo method, AttributeEntity attributeEntity)
		{
			Method = method;
			Parameters = method.GetParameters();
			AttrEntity = attributeEntity;
		}
	}
}
