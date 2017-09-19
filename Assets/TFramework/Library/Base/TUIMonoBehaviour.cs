﻿namespace TFramework.Base
{
    public abstract class TUIMonoBehaviour : TMonoBehaviour
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public abstract void Initalization();
        /// <summary>
        /// 查找对象
        /// </summary>
        public abstract void FindChild();

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}