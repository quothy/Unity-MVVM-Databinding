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

#if UNITY_EDITOR
        ThemeRecord Record { get; }
        int ItemId { get; }
        // NOTE: We're using object here only because this is strictly an Editor time call. 
        void Editor_ForceUpdateItemValue(object value);
#endif
    }
}