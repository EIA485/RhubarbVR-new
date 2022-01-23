﻿using System;
using System.Collections.Generic;
using System.Reflection;

using RhuEngine.DataStructure;
using RhuEngine.Datatypes;

using StereoKit;

namespace RhuEngine.WorldObjects
{
	public class SyncObjectSerializerObject
	{
		public bool NetSync { get; private set; }

		public SyncObjectSerializerObject(bool netSync) {
			NetSync = netSync;
		}

		public static DataNodeGroup CommonSerialize(IWorldObject @object) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			return obj;
		}

		public DataNodeGroup CommonAbstractListSerialize(IWorldObject @object, IEnumerable<ISyncObject> worldObjects) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			var list = new DataNodeList();
			foreach (var val in worldObjects) {
				if (!val.IsRemoved) {
					var tip = val.Serialize(this);
					var listobj = new DataNodeGroup();
					if (tip != null) {
						listobj.SetValue("Value", tip);
					}
					//Need To add Constant Type Strings for better compression 
					listobj.SetValue("Type", new DataNode<string>(val.GetType().FullName));
					list.Add(listobj);
				}
			}
			obj.SetValue("list", list);
			return obj;
		}

		public DataNodeGroup CommonListSerialize(IWorldObject @object, IEnumerable<ISyncObject> worldObjects) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			var list = new DataNodeList();
			foreach (var val in worldObjects) {
				if (!val.IsRemoved) {
					var tip = val.Serialize(this);
					if (tip != null) {
						list.Add(tip);
					}
				}
			}
			obj.SetValue("list", list);
			return obj;
		}

		public static DataNodeGroup CommonRefSerialize(IWorldObject @object, NetPointer target) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			var Value = new DataNode<NetPointer>(target);
			obj.SetValue("targetPointer", Value);
			return obj;
		}
		public static DataNodeGroup CommonValueSerialize<T>(IWorldObject @object, T value) {
			var obj = new DataNodeGroup();
			var refID = new DataNode<NetPointer>(@object.Pointer);
			obj.SetValue("Pointer", refID);
			var Value = typeof(T).IsEnum ? new DataNode<int>((int)(object)value) : (IDataNode)new DataNode<T>(value);
			obj.SetValue("Value", Value);
			return obj;
		}
		public DataNodeGroup CommonWorkerSerialize(IWorldObject @object) {
			var fields = @object.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			DataNodeGroup obj = null;
			if (@object.Persistence || NetSync) {
				if (!NetSync) {
					if (typeof(ISyncObject).IsAssignableFrom(@object.GetType())) {
						var castObject = @object as ISyncObject;
						try {
							castObject.OnSave();
						}
						catch (Exception e) {
							Log.Warn($"Failed to save {@object.GetType().GetFormattedName()} Error: {e}");
						}
					}
				}
				obj = new DataNodeGroup();
				foreach (var field in fields) {
					if (typeof(ISyncObject).IsAssignableFrom(field.FieldType) && ((field.GetCustomAttributes(typeof(NoSaveAttribute), false).Length <= 0) || (NetSync && (field.GetCustomAttributes(typeof(NoSyncAttribute), false).Length <= 0)))) {
						try {
							if (!@object.IsRemoved) {
								obj.SetValue(field.Name, ((ISyncObject)field.GetValue(@object)).Serialize(this));
							}
						}
						catch (Exception e) {
							throw new Exception($"Failed to serialize {@object.GetType()}, field {field.Name}, field type {field.FieldType.GetFormattedName()}. Error: {e}");
						}
					}
				}
				var refID = new DataNode<NetPointer>(@object.Pointer);
				obj.SetValue("Pointer", refID);
			}
			return obj;
		}
	}
}
