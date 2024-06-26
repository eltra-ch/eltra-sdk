﻿using EltraUiCommon.Controls;
using EltraUiCommon.Controls.Parameters;
using EltraMauiCommon.Framework;

namespace EltraMauiCommon.Controls.Parameters
{
    public class XamParameterLabelViewModel : ParameterLabelViewModel
    {
        public XamParameterLabelViewModel(ToolViewBaseModel parent, string uniqueId)
            : base(parent, uniqueId)
        {
            Init(new InvokeOnMainThread());
        }
    }
}