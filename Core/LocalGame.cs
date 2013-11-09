using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Gem
{
    public class LocalGame : IGame
    {
        public Input Input { get; set; }
        public Main Main { get; set; }

        Simulation simulation;

        Render.RenderModule renderModule = null;
        InputActionHandler actionHandler = new InputActionHandler();
        ObjectClickHandler clickHandler = new ObjectClickHandler();
        UInt32 hoverObject = 0;
        Gui.GuiModule guiModule = null;
        float elapsedTime = 0.0f;

        RealtimeCSG.CSGNode csgNode = null;
        Euler spinEuler = new Euler();
        Euler rootEuler = new Euler();
        //Render.LeafNode renderNode = null;
        RealtimeCSG.CSGNode spinner = null;

        public LocalGame()
        {
        }

        public void Begin()
        {
            guiModule = new Gui.GuiModule(Main.GraphicsDevice, Main.Input);
            //inputModule = new InputModule(Main.Input);

            simulation = new Simulation(Main.Content, new PropertySet(
                "episode-name", "Content", "server", null));
            simulation.debugOutput += (s) => { Main.Write(s); };
            simulation.Content.FrontLoadTextures();
            Main.Write("Started menu simulation\n");

            renderModule = new Render.RenderModule(Main.GraphicsDevice, simulation.Content);
            simulation.modules.Add(renderModule);
            //simulation.modules.Add(inputModule);
            simulation.modules.Add(guiModule);

            simulation.beginSimulation();

            Main.Input.ClearBindings();
            Main.Input.AddAxis("primary", new MouseAxisBinding());

            var icosohedron = Geo.Ico.Icosahedron.Generate();
            //icosohedron = icosohedron.Subdivide();
            var icoMesh = icosohedron.GenerateMesh(2.0f);
            var icoBrush = Geo.Csg.PlanesFromMesh(icoMesh);

            spinner = //new RealtimeCSG.CSGNode(RealtimeCSG.CSGNodeType.Addition,
                new RealtimeCSG.CSGNode(new RealtimeCSG.StaticBrushGenerator(Geo.Csg.CreatePrism(6, Vector3.UnitY, Vector3.UnitZ, 10.0f)));
            spinner.TextureProjection = new Geo.PlanarProjection(
                new RealtimeCSG.Plane(0.0f, 1.0f, 0.0f, 1.0f), Vector3.UnitZ, 10.0f);
            spinner.Texture = Main.Content.Load<Texture2D>("Content/swirl");

            //spinner = new Geo.Csg.CsgTransformationNode(
            //        new Geo.Csg.OperationNode(RealtimeCSG.CSGNodeType.Addition,
            //            new Geo.Csg.BrushNode(Geo.Csg.CreatePrism(6, Vector3.UnitY, Vector3.UnitZ, 10.0f)),
            //            new Geo.Csg.OperationNode(RealtimeCSG.CSGNodeType.Addition,
            //                new Geo.Csg.BrushNode(Geo.Csg.CreatePrism(6, Vector3.UnitX, Vector3.UnitY, 10.0f)),
            //                new Geo.Csg.BrushNode(Geo.Csg.CreatePrism(6, Vector3.UnitZ, Vector3.UnitX, 10.0f)))),
            //        spinEuler);

            var icoNode = new RealtimeCSG.CSGNode(icoBrush);
            csgNode = new RealtimeCSG.CSGNode(RealtimeCSG.CSGNodeType.Subtraction,
                icoNode,
                spinner);
            icoNode.Texture = Main.Content.Load<Texture2D>("Content/swirl");
            icoNode.TextureProjection = new Geo.PlanarProjection(
                new RealtimeCSG.Plane(0, 0, 1, 0), new Vector3(0, 1, 0), 10.0f);
            //this.csgNode = new Geo.Csg.OperationNode(RealtimeCSG.CSGNodeType.Subtraction,
            //    new Geo.Csg.BrushTransformationNode(
            //        new Geo.Csg.BrushNode(icoBrush),
            //        rootEuler),
            //        spinner);


            //this.csgNode = new Geo.Csg.BrushNode(icoBrush);
            var list = new List<RealtimeCSG.CSGNode>();
            list.Add(csgNode);
            var result = RealtimeCSG.CSGCategorization.ProcessCSGNodes(csgNode, list);
            var gemCube = Geo.Csg.ConvertToMesh(csgNode.cachedMesh);
            gemCube = Geo.Gen.FacetCopy(gemCube);

            var cubeEuler = new Euler();
            var renderNode = new Render.CsgMeshLeafNode(csgNode, cubeEuler);
            simulation.AddEntity(250, new Render.SceneGraphRoot(renderNode));
            //var guiEuler = new Euler();
            //var gui = new Gui.SceneNode(128, 128, guiEuler);
            //simulation.AddEntity(61440, new Render.SceneGraphRoot(gui));

            /*guiEuler.Scale = new Vector3(4, 4, 4);
            var button = new Gui.UIItem(new Rectangle(16,16,64,32));
            button.settings.Upsert("bg-color", new Vector3(0.3f, 0.3f, 0.3f));
            button.hoverSettings = new PropertySet("bg-color", new Vector3(0.9f, 0.9f, 0.9f));
            gui.uiRoot.AddChild(button);*/

            Input.AddBinding("left", new KeyboardBinding(Keys.A, KeyBindingType.Held));
            Input.AddBinding("up", new KeyboardBinding(Keys.W, KeyBindingType.Held));
            Input.AddBinding("right", new KeyboardBinding(Keys.D, KeyBindingType.Held));
            Input.AddBinding("down", new KeyboardBinding(Keys.S, KeyBindingType.Held));

            actionHandler.MapAction("left", () => { renderModule.Camera.Yaw(2 * elapsedTime); });
            actionHandler.MapAction("right", () => { renderModule.Camera.Yaw(-2 * elapsedTime); });
            actionHandler.MapAction("up", () => { renderModule.Camera.Pitch(2 * elapsedTime); });
            actionHandler.MapAction("down", () => { renderModule.Camera.Pitch(-2 * elapsedTime); });

            Input.AddBinding("click", new MouseButtonBinding("LeftButton", KeyBindingType.Pressed));
            actionHandler.MapAction("click", () => { clickHandler.FireMapping(hoverObject, simulation); });

            //clickHandler.AddHandler(250, l => { renderNode.Color = Vector3.UnitX; });
        }

        public void End()
        {
            simulation.endSimulation();
        }

        float totalSeconds = 0.0f;
        public void Update(float elapsedSeconds)
        {
            totalSeconds += elapsedSeconds;
            elapsedTime = elapsedSeconds;
            actionHandler.CheckAndFireMappings(Main.Input, simulation);
            simulation.update(elapsedSeconds);

            spinner.Transformation.Orientation.X += elapsedSeconds;
            //spinEuler.Orientation.Y += elapsedSeconds;
            //spinEuler.Orientation.Z += elapsedSeconds;
            //rootEuler.Orientation.Z -= elapsedSeconds;
            //spinner.Transformation = spinEuler.Transform;
            spinner.Color = Vector3.UnitX;
            csgNode.DirtyTree();
            csgNode.Color = Vector3.UnitY;

            float scale = ((float)System.Math.Sin(totalSeconds) + 4.0f) * 0.5f;
            //rootEuler.Scale = new Vector3(scale, scale, scale);

            var list = new List<RealtimeCSG.CSGNode>();
            list.Add(csgNode);
            var result = RealtimeCSG.CSGCategorization.ProcessCSGNodes(csgNode, list);
            //var gemCube = Geo.Csg.ConvertToMesh(csgNode.cachedMesh);
            //gemCube = Geo.Gen.FacetCopy(gemCube);

            //renderNode.Mesh = gemCube;
            //renderNode.Mesh.PrepareLineIndicies();
        }

        public void Draw(float elapsedSeconds)
        {
            guiModule.DrawGuis();
            renderModule.PreDraw(elapsedSeconds);
            hoverObject = renderModule.MousePick(Input.QueryAxis("primary"));
            renderModule.Draw();
        }
    }
}
