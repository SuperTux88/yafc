using System;
using System.Collections.Generic;
using System.Numerics;
using YAFC.Model;
using YAFC.UI;

namespace YAFC
{
    public abstract class ProjectPageView : Scrollable
    {
        protected ProjectPageView() : base(true, true, false)
        {
            headerContent = new ImGui(BuildHeader, default, RectAllocator.LeftAlign);
            bodyContent = new ImGui(BuildContent, default, RectAllocator.LeftAlign, true);
        }

        public readonly ImGui headerContent;
        public readonly ImGui bodyContent;
        private float contentWidth, headerHeight, contentHeight;
        protected abstract void BuildHeader(ImGui gui);
        protected abstract void BuildContent(ImGui gui);

        public virtual void Rebuild(bool visualOnly = false)
        {
            headerContent.Rebuild(); 
            bodyContent.Rebuild();
        }

        public abstract void SetModel(ProjectPage page);

        public void Build(ImGui gui, Vector2 visibleSize)
        {
            if (gui.isBuilding)
            {
                gui.spacing = 0f;
                var position = gui.AllocateRect(0f, 0f, 0f).Position;
                var headerSize = headerContent.CalculateState(visibleSize.X-0.5f, gui.pixelsPerUnit);
                contentWidth = headerSize.X;
                headerHeight = headerSize.Y;
                var headerRect = gui.AllocateRect(visibleSize.X, headerHeight);
                position.Y += headerHeight;
                var contentSize = bodyContent.CalculateState(visibleSize.X-0.5f, gui.pixelsPerUnit);
                if (contentSize.X > contentWidth)
                    contentWidth = contentSize.X;
                contentHeight = contentSize.Y;
                gui.DrawPanel(headerRect, headerContent);
            }
            else
                gui.AllocateRect(contentWidth, headerHeight);
            
            base.Build(gui, visibleSize.Y - headerHeight);
        }

        protected override Vector2 MeasureContent(Rect rect, ImGui gui)
        {
            return new Vector2(contentWidth, contentHeight);
        }

        protected override void PositionContent(ImGui gui, Rect viewport)
        {
            headerContent.offset = new Vector2(-scrollX, 0);
            bodyContent.offset = -scroll2d;
            gui.DrawPanel(viewport, bodyContent);
        }

        public abstract void CreateModelDropdown(ImGui gui1, Type type, Project project, ref bool close);
    }

    public abstract class ProjectPageView<T> : ProjectPageView where T : ProjectPageContents
    {
        protected T model;
        protected ProjectPage projectPage;

        protected override void BuildHeader(ImGui gui)
        {
            if (projectPage?.modelError != null && gui.BuildErrorRow(projectPage.modelError))
                projectPage.modelError = null;
        }

        public override void SetModel(ProjectPage page)
        {
            if (model != null)
                projectPage.contentChanged -= Rebuild;
            projectPage = page;
            model = page?.content as T;
            if (model != null)
            {
                projectPage.contentChanged += Rebuild;
                Rebuild();
            }
        }
    }
}