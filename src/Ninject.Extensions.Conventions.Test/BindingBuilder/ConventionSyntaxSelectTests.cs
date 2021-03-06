//-------------------------------------------------------------------------------
// <copyright file="ConventionSyntaxSelectTests.cs" company="Ninject Project Contributors">
//   Copyright (c) 2009-2011 Ninject Project Contributors
//   Authors: Remo Gloor (remo.gloor@gmail.com)
//           
//   Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
//   you may not use this file except in compliance with one of the Licenses.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//   or
//       http://www.microsoft.com/opensource/licenses.mspx
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

#if !NO_MOQ
namespace Ninject.Extensions.Conventions.BindingBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Moq;

    using Ninject.Extensions.Conventions.Fakes;
    using Ninject.Extensions.Conventions.Fakes.NormalClasses;
    using Xunit;
    using Xunit.Extensions;

    public class ConventionSyntaxSelectTests
    {
        private readonly Mock<IConventionBindingBuilder> conventionBindingBuilderMock;

        private readonly ConventionSyntax testee;

        private readonly Mock<ITypeFilter> typeFilterMock;

        private Func<Type, bool> filter;

        public ConventionSyntaxSelectTests()
        {
            this.conventionBindingBuilderMock = new Mock<IConventionBindingBuilder>();
            this.typeFilterMock = new Mock<ITypeFilter>();
#if !NO_ASSEMBLY_SCANNING
            this.testee = new ConventionSyntax(this.conventionBindingBuilderMock.Object, null, this.typeFilterMock.Object, null);
#else
            this.testee = new ConventionSyntax(this.conventionBindingBuilderMock.Object, this.typeFilterMock.Object, null);
#endif
        }

        [Fact]
        public void Select_CallsBuilder_WithTheGivenFilter()
        {
            Func<Type, bool> expectedFilter = t => true;

            this.testee.Select(expectedFilter);

            this.conventionBindingBuilderMock.Verify(b => b.Where(expectedFilter));
        }

        [Fact]
        public void SelectAllTypes_CallsBuilder_WithAFilterReturningTrueForAllTypes()
        {
            this.SetupStoreFilter();

            this.testee.SelectAllTypes();

            this.filter.Should().NotBeNull();
            this.filter(typeof(List<int>)).Should().BeTrue();
            this.filter(typeof(DefaultConvention)).Should().BeTrue();
            this.filter(typeof(IFoo)).Should().BeTrue();
            this.filter(typeof(AbstractClassWithManyInterfaces)).Should().BeTrue();
        }

        [Fact]
        public void SelectAllClasses_CallsBuilder_WithAFilterReturningTrueForAllNoneAbstractClasses()
        {
            this.SetupStoreFilter();

            this.testee.SelectAllClasses();

            this.filter.Should().NotBeNull();
            this.filter(typeof(List<int>)).Should().BeTrue();
            this.filter(typeof(DefaultConvention)).Should().BeTrue();
            this.filter(typeof(IFoo)).Should().BeFalse();
            this.filter(typeof(AbstractClassWithManyInterfaces)).Should().BeFalse();
        }
        
        [Fact]
        public void SelectAbstractClasses_CallsBuilder_WithAFilterReturningTrueForAllNoneAbstractClasses()
        {
            this.SetupStoreFilter();

            this.testee.SelectAllAbstractClasses();

            this.filter.Should().NotBeNull();
            this.filter(typeof(List<int>)).Should().BeFalse();
            this.filter(typeof(DefaultConvention)).Should().BeFalse();
            this.filter(typeof(IFoo)).Should().BeFalse();
            this.filter(typeof(AbstractClassWithManyInterfaces)).Should().BeTrue();
        }
        
        [Fact]
        public void SelectAllIncludingAbstractClasses_CallsBuilder_WithAFilterReturningTrueForAllNoneAbstractClasses()
        {
            this.SetupStoreFilter();

            this.testee.SelectAllIncludingAbstractClasses();

            this.filter.Should().NotBeNull();
            this.filter(typeof(List<int>)).Should().BeTrue();
            this.filter(typeof(DefaultConvention)).Should().BeTrue();
            this.filter(typeof(IFoo)).Should().BeFalse();
            this.filter(typeof(AbstractClassWithManyInterfaces)).Should().BeTrue();
        }  
      
        [Fact]
        public void SelectAllInterfaces_CallsBuilder_WithAFilterReturningTrueForAllNoneAbstractClasses()
        {
            this.SetupStoreFilter();

            this.testee.SelectAllInterfaces();

            this.filter.Should().NotBeNull();
            this.filter(typeof(List<int>)).Should().BeFalse();
            this.filter(typeof(DefaultConvention)).Should().BeFalse();
            this.filter(typeof(IFoo)).Should().BeTrue();
            this.filter(typeof(AbstractClassWithManyInterfaces)).Should().BeFalse();
        }
        
        [Theory]
        [InlineData(new[] { "SomeNamespace", "MatchingNamespace" }, new[] { "MatchingNamespace" }, true)]
        [InlineData(new[] { "SomeNamespace", "OtherNamespace" }, new string[0], false)]
        public void InNamespacesWithParams_CallsBuilder_WithAFilterSelectingTypesThatHaveAnyMatchingNamespace(
            string[] namespaces, 
            string[] matchingNameSpaces, 
            bool expectedResult)
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsInNameSpace(type, matchingNameSpaces);

            this.testee.InNamespaces(namespaces);
            var result = this.filter(type);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(new[] { "SomeNamespace", "MatchingNamespace" }, new[] { "MatchingNamespace" }, true)]
        [InlineData(new[] { "SomeNamespace", "OtherNamespace" }, new string[0], false)]
        public void InNamespaces_CallsBuilder_WithAFilterSelectingTypesThatHaveAnyMatchingNamespace(
            string[] namespaces,
            string[] matchingNameSpaces,
            bool expectedResult)
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsInNameSpace(type, matchingNameSpaces);

            this.testee.InNamespaces(namespaces.AsEnumerable());
            var result = this.filter(type);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(new[] { typeof(DefaultConvention), typeof(IList<>) }, new[] { "System.Collections.Generic" }, true)]
        [InlineData(new[] { typeof(DefaultConvention), typeof(IList<>) }, new string[0], false)]
        public void InNamespaceOf_CallsBuilder_WithAFilterSelectingTypesThatHaveAnyMatchingNamespace(
            Type[] namespaceTypes,
            string[] matchingNameSpaces,
            bool expectedResult)
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsInNameSpace(type, matchingNameSpaces);

            this.testee.InNamespaceOf(namespaceTypes);
            var result = this.filter(type);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(new[] { "System" }, true)]
        [InlineData(new string[0], false)]
        public void InNamespaceOfGeneric_CallsBuilder_WithAFilterSelectingTypesThatHaveAnyMatchingNamespace(
            string[] matchingNameSpaces,
            bool expectedResult)
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsInNameSpace(type, matchingNameSpaces);

            this.testee.InNamespaceOf<int>();
            var result = this.filter(type);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(new[] { "SomeNamespace", "MatchingNamespace" }, new[] { "SomeNamespace", "MatchingNamespace" }, false)]
        [InlineData(new[] { "SomeNamespace", "MatchingNamespace" }, new[] { "MatchingNamespace" }, false)]
        [InlineData(new[] { "SomeNamespace", "OtherNamespace" }, new string[0], true)]
        public void NotInNamespacesWithParams_CallsBuilder_WithAFilterSelectingTypesThatHaveNoMatchingNamespace(
            string[] namespaces,
            string[] matchingNameSpaces,
            bool expectedResult)
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsInNameSpace(type, matchingNameSpaces);

            this.testee.NotInNamespaces(namespaces);
            var result = this.filter(type);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(new[] { "SomeNamespace", "MatchingNamespace" }, new[] { "SomeNamespace", "MatchingNamespace" }, false)]
        [InlineData(new[] { "SomeNamespace", "MatchingNamespace" }, new[] { "MatchingNamespace" }, false)]
        [InlineData(new[] { "SomeNamespace", "OtherNamespace" }, new string[0], true)]
        public void NotInNamespaces_CallsBuilder_WithAFilterSelectingTypesThatHaveNoMatchingNamespace(
            string[] namespaces,
            string[] matchingNameSpaces,
            bool expectedResult)
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsInNameSpace(type, matchingNameSpaces);

            this.testee.NotInNamespaces(namespaces.AsEnumerable());
            var result = this.filter(type);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(new[] { typeof(object), typeof(IList<>) }, new[] { "System", "System.Collections.Generic" }, false)]
        [InlineData(new[] { typeof(object), typeof(IList<>) }, new[] { "System.Collections.Generic" }, false)]
        [InlineData(new[] { typeof(object), typeof(IList<>) }, new string[0], true)]
        public void NotInNamespaceOf_CallsBuilder_WithAFilterSelectingTypesThatHaveNoMatchingNamespace(
            Type[] namespaceTypes,
            string[] matchingNameSpaces,
            bool expectedResult)
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsInNameSpace(type, matchingNameSpaces);

            this.testee.NotInNamespaceOf(namespaceTypes);
            var result = this.filter(type);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(new[] { "System" }, false)]
        [InlineData(new string[0], true)]
        public void NotInNamespaceOfGeneric_CallsBuilder_WithAFilterSelectingTypesThatHaveNoMatchingNamespace(
            string[] matchingNameSpaces,
            bool expectedResult)
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsInNameSpace(type, matchingNameSpaces);

            this.testee.NotInNamespaceOf<int>();
            var result = this.filter(type);

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void InheritedFromAnyWithParams_CallsBuilder_WithAFilterAskingTypeFilterIfItIsInherited()
        {
            var givenTypes = new[] { typeof(object), typeof(double) };
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsTypeInheritedFromAny(type, givenTypes);

            this.testee.InheritedFromAny(givenTypes);
            var result = this.filter(type);

            result.Should().BeTrue();
            this.typeFilterMock.VerifyAll();
        }

        [Fact]
        public void InheritedFromAny_CallsBuilder_WithAFilterAskingTypeFilterIfItIsInherited()
        {
            var givenTypes = new[] { typeof(object), typeof(double) };
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsTypeInheritedFromAny(type, givenTypes);

            this.testee.InheritedFromAny(givenTypes.AsEnumerable());
            var result = this.filter(type);

            result.Should().BeTrue();
            this.typeFilterMock.VerifyAll();
        }

        [Fact]
        public void InheritedFrom_CallsBuilder_WithAFilterAskingTypeFilterIfItIsInherited()
        {
            var givenType = typeof(object);
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsTypeInheritedFromAny(type, givenType);

            this.testee.InheritedFrom(givenType);
            var result = this.filter(type);

            result.Should().BeTrue();
            this.typeFilterMock.VerifyAll();
        }

        [Fact]
        public void InheritedFromGeneric_CallsBuilder_WithAFilterAskingTypeFilterIfItIsInherited()
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupIsTypeInheritedFromAny(type, typeof(object));

            this.testee.InheritedFrom<object>();
            var result = this.filter(type);

            result.Should().BeTrue();
            this.typeFilterMock.VerifyAll();
        }

        [Fact]
        public void WithAttribute_CallsBuilder_WithAFilterAskingIfTypeHasAttribute()
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupHasAttribute(type, typeof(TestAttribute));

            this.testee.WithAttribute<TestAttribute>();
            var result = this.filter(type);

            result.Should().BeTrue();
            this.typeFilterMock.VerifyAll();
        }

        [Fact]
        public void WithoutAttribute_CallsBuilder_WithAFilterAskingIfTypeHasAttribute()
        {
            var type = typeof(int);

            this.SetupStoreFilter();
            this.SetupHasAttribute(type, typeof(TestAttribute));

            this.testee.WithoutAttribute<TestAttribute>();
            var result = this.filter(type);

            result.Should().BeFalse();
            this.typeFilterMock.VerifyAll();
        }
        
        [Fact]
        public void WithAttribute_NoneGeneric_CallsBuilder_WithAFilterAskingIfTypeHasAttribute()
        {
            var type = typeof(int);
            var attributeType = typeof(TestAttribute);

            this.SetupStoreFilter();
            this.SetupHasAttribute(type, attributeType);

            this.testee.WithAttribute(attributeType);
            var result = this.filter(type);

            result.Should().BeTrue();
            this.typeFilterMock.VerifyAll();
        }

        [Fact]
        public void WithoutAttribute_NoneGeneric_CallsBuilder_WithAFilterAskingIfTypeHasAttribute()
        {
            var type = typeof(int);
            var attributeType = typeof(TestAttribute);

            this.SetupStoreFilter();
            this.SetupHasAttribute(type, attributeType);

            this.testee.WithoutAttribute(attributeType);
            var result = this.filter(type);

            result.Should().BeFalse();
            this.typeFilterMock.VerifyAll();
        }
        
        [Fact]
        public void WithAttribute_WithMatcher_CallsBuilder_WithAFilterAskingIfTypeHasAttribute()
        {
            var type = typeof(int);
            Func<TestAttribute, bool> matcher = a => a.TestValue == 1;

            this.SetupStoreFilter();
            this.SetupHasAttribute(type, matcher);

            this.testee.WithAttribute(matcher);
            var result = this.filter(type);

            result.Should().BeTrue();
            this.typeFilterMock.VerifyAll();
        }
        
        [Fact]
        public void WithoutAttribute_WithMatcher_CallsBuilder_WithAFilterAskingIfTypeHasAttribute()
        {
            var type = typeof(int);
            Func<TestAttribute, bool> matcher = a => a.TestValue == 1;

            this.SetupStoreFilter();
            this.SetupHasAttribute(type, matcher);

            this.testee.WithoutAttribute(matcher);
            var result = this.filter(type);

            result.Should().BeFalse();
            this.typeFilterMock.VerifyAll();
        }
        
        private void SetupIsTypeInheritedFromAny(Type type, params Type[] givenTypes)
        {
            this.typeFilterMock
                .Setup(f => f.IsTypeInheritedFromAny(type, givenTypes))
                .Returns(true)
                .Verifiable();
        }

        private void SetupHasAttribute(Type type, Type attributeType)
        {
            this.typeFilterMock
                .Setup(f => f.HasAttribute(type, attributeType))
                .Returns(true)
                .Verifiable();
        }

        private void SetupHasAttribute(Type type, Func<TestAttribute, bool> matcher)
        {
            this.typeFilterMock
                .Setup(f => f.HasAttribute(type, matcher))
                .Returns(true)
                .Verifiable();
        }
        
        private void SetupIsInNameSpace(Type type, params string[] matchingNamespaces)
        {
            this.typeFilterMock
                .Setup(f => f.IsTypeInNamespace(type, It.IsAny<string>()))
                .Returns<Type, string>((t, ns) => matchingNamespaces.Contains(ns));
        }

        private void SetupStoreFilter()
        {
            this.conventionBindingBuilderMock
                .Setup(b => b.Where(It.IsAny<Func<Type, bool>>()))
                .Callback<Func<Type, bool>>(f => this.filter = f);
        }
    }
}
#endif