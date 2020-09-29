namespace FoodTruck.Areas.Administrer.Controllers
{
    public class ControllerParent : FoodTruck.Controllers.ControllerParent
    {
        public ControllerParent()
        {
            ViewBag.PanierLatteralDesactive = true;
            ViewBag.ModeAdmin = true;
            ViewBag.ModeClient = false;
        }
    }
}