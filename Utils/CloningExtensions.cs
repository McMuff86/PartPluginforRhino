using System.Collections.Generic;
using BauteilPlugin.Models;

namespace BauteilPlugin.Utils
{
    public static class CloningExtensions
    {
        public static EdgeMachiningOperation CloneWithoutCopySuffix(this EdgeMachiningOperation operation)
        {
            var aClone = new EdgeMachiningOperation
            {
                Id = System.Guid.NewGuid(),
                Name = operation.Name, // Explicitly avoids adding "(Copy)"
                OperationType = operation.OperationType,
                ExecutionOrder = operation.ExecutionOrder
            };
            
            foreach (var kvp in operation.Parameters)
            {
                aClone.Parameters.Add(kvp.Key, kvp.Value); // Shallow copy
            }
            return aClone;
        }
    }
} 