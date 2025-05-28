/*Copyright 2025 Emmi Poutanen

This file is part of "Juttunurkka".

Juttunurkka is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.

Juttunurkka is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Juttunurkka.  If not, see <https://www.gnu.org/licenses/>.
*/


namespace PrototypeUnitTest
{
    public class UnitTestBase
    {
        // Example unit test. Move tests to suitable folders, for example based on class.
        [Fact]
        public void GetDefaultSurvey_ReturnsCorrectValues()
        {
            var result = Prototype.Survey.GetDefaultSurvey();
            Assert.NotNull(result);
        }
    }
}