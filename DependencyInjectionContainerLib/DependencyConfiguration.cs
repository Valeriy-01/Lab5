using System;
using System.Collections.Generic;
using System.Linq;

namespace DependencyInjectionContainer
{
    public class DependencyConfiguration
    {
        private static readonly Dictionary<Type, List<Dependency>> dependencyList = new Dictionary<Type, List<Dependency>>();
        public void Register<TDependency, TImplementation>(LifeType lifeType)
            where TDependency : class
            where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), lifeType);
        }

        private void Register(Type dependencyType, Type implementationType, LifeType lifeType)
        {
            if (dependencyType == null)
            {
                throw new ArgumentNullException(nameof(dependencyType));
            }
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            if (!implementationType.IsClass)
            {
                throw new ArgumentException($"{implementationType} must be a reference type");
            }
            if (implementationType.IsAbstract || implementationType.IsInterface)
            {
                throw new ArgumentException($"{implementationType} must be not abstract");
            }

            if (dependencyType.IsAssignableFrom(implementationType)
                 || implementationType.IsGenericTypeDefinition && dependencyType.IsGenericTypeDefinition &&
                IsAssignableFromGeneric(dependencyType, implementationType))
            {
                Dependency dependency = new Dependency(implementationType, lifeType);
                if (dependencyList.ContainsKey(dependencyType))
                {
                    dependencyList[dependencyType].Add(dependency);
                }
                else
                {
                    List<Dependency> implementations = new List<Dependency>();
                    implementations.Add(dependency);
                    dependencyList.Add(dependencyType, implementations);
                }
            }
            else
            {
                throw new ArgumentException($"{implementationType} must be non abstract and must subtype of {dependencyType}");
            }
        }
        private IEnumerable<Type> GetBaseTypes(Type type)
        {
            for (var baseType = type; baseType != null; baseType = baseType.BaseType)
                yield return baseType;

            var interfaceTypes = type.GetInterfaces();

            foreach (var interfaceType in interfaceTypes)
                yield return interfaceType;
        }

        private Type GetTypeDefinition(Type type) =>
            type.IsGenericType ? type.GetGenericTypeDefinition() : type;

        private bool IsAssignableFromGeneric(Type dependencyType, Type implementationType)
        {
            var baseTypes = GetBaseTypes(GetTypeDefinition(implementationType));
            return baseTypes
                .Select(GetTypeDefinition)
                .Contains(GetTypeDefinition(dependencyType));
        }

        public bool GetDependency(Type dependencyType, out Dependency dependency)
        {
            if (dependencyList.TryGetValue(dependencyType, out var dependencies))
            {
                dependency = dependencies.First();
                return true;
            } 
            else
            {
                dependency = null;
                return false;
            }
        }

        public bool GetAllDependencies(Type dependencyType, out IEnumerable<Dependency> dependencies)
        {
            bool isFound = dependencyList.TryGetValue(dependencyType, out var newDependencyList);
            if (isFound)
            {
                dependencies = newDependencyList;
            }
            else
            {
                dependencies = null;
            }
            return isFound;
        }
    }
}
