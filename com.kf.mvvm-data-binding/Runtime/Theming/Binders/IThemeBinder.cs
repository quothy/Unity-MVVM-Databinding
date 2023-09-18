// Copyright (c) 2023 Katie Fremont
// Licensed under the MIT license

namespace MVVMDatabinding.Theming
{
    public interface IThemeBinder
    {
        string Name { get; }
        bool DataRecordValid { get; }

        void Bind();
        void Unbind();

        void OnThemeItemUpdate(IDataSource dataSource, int itemId);
    }
}