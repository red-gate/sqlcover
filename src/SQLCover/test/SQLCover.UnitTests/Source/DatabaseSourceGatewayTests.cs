﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SQLCover.Gateway;
using SQLCover.Source;
using SqlServerVersion = SQLCover.Objects.SqlServerVersion;

namespace SQLCover.UnitTests.Source
{
    [TestFixture]
    public class DatabaseSourceGatewayTests
    {
        
        [TestCase(SqlServerVersion.Sql100)]
        [TestCase(SqlServerVersion.Sql110)]
        [TestCase(SqlServerVersion.Sql120)]
        [TestCase(SqlServerVersion.Sql130)]
        [TestCase(SqlServerVersion.Sql140)]
        public void GetVersion_Detects_SQL_Version(SqlServerVersion expected)
        {
            var gateway = GetGateway(expected);
            var source = new DatabaseSourceGateway(gateway.Object);
            Assert.AreEqual(expected, source.GetVersion());
        }

        [Test]
        public void GetVersion_WhenFailedToRetrieveValue_ReturnsSql140()
        {
            var gateway = new Mock<DatabaseGateway>();
            gateway.Setup(x => x.GetString(It.IsAny<string>())).Returns((string)null);
            var source = new DatabaseSourceGateway(gateway.Object);
            Assert.AreEqual(SqlServerVersion.Sql140, source.GetVersion());
        }

        private Mock<DatabaseGateway> GetGateway(SqlServerVersion expected)
        {
            var gateway = new Mock<DatabaseGateway>();
            
            switch (expected)
            {
                case SqlServerVersion.Sql90:
                    gateway.Setup(p => p.GetString(It.IsAny<string>())).Returns("90");
                    break;
                case SqlServerVersion.Sql100:
                    gateway.Setup(p => p.GetString(It.IsAny<string>())).Returns("100");
                    break;
                case SqlServerVersion.Sql110:
                    gateway.Setup(p => p.GetString(It.IsAny<string>())).Returns("110");
                    break;
                case SqlServerVersion.Sql120:
                    gateway.Setup(p => p.GetString(It.IsAny<string>())).Returns("120");
                    break;
                case SqlServerVersion.Sql130:
                    gateway.Setup(p => p.GetString(It.IsAny<string>())).Returns("130");
                    break;
                case SqlServerVersion.Sql140:
                    gateway.Setup(p => p.GetString(It.IsAny<string>())).Returns("140");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(expected), expected, null);
            }

            return gateway;
        }



    }
}
    