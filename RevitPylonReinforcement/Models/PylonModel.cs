using Autodesk.Revit.DB;

namespace RevitPylonReinforcement.Models {
    internal class PylonModel {
        public PylonModel(FamilyInstance familyInstance) {

            Pylon = familyInstance;

            Analize();
        }


        public FamilyInstance Pylon { get; set; }

        public XYZ Bottom1Pt { get; set; }
        public XYZ Bottom2Pt { get; set; }
        public XYZ Bottom3Pt { get; set; }
        public XYZ Bottom4Pt { get; set; }
        public XYZ Top1Pt { get; set; }
        public XYZ Top2Pt { get; set; }
        public XYZ Top3Pt { get; set; }
        public XYZ Top4Pt { get; set; }
        public XYZ BottomMiddlePt { get; set; }
        public XYZ TopMiddlePt { get; set; }
        public Curve DrivingCurve { get; set; }


        private void Analize() {

            // Получаем объект выдавливания профиля вдоль прямой
            SweptProfile sweptProfile = Pylon.GetSweptProfile();
            // Определение линии вдоль которой происходит выдавливание
            DrivingCurve = sweptProfile.GetDrivingCurve();
            // Ее нижняя и верхняя точки
            BottomMiddlePt = DrivingCurve.GetEndPoint(0);
            TopMiddlePt = DrivingCurve.GetEndPoint(1);


            // Линии/точки объекта в данном случае получаются вокруг точки (0,0,0) и без учета вращения объекта
            // Получаем линии выдавливаемого профиля вокруг (0,0,0)
            Line bl1AroundZero = sweptProfile.GetSweptProfile().Curves.get_Item(0) as Line;
            Line bl2AroundZero = sweptProfile.GetSweptProfile().Curves.get_Item(1) as Line;
            Line bl3AroundZero = sweptProfile.GetSweptProfile().Curves.get_Item(2) as Line;
            Line bl4AroundZero = sweptProfile.GetSweptProfile().Curves.get_Item(3) as Line;

            // Получаем нижние точки объекта вокруг (0,0,0)
            XYZ b1AroundZero = bl1AroundZero.GetEndPoint(0);
            XYZ b2AroundZero = bl2AroundZero.GetEndPoint(0);
            XYZ b3AroundZero = bl3AroundZero.GetEndPoint(0);
            XYZ b4AroundZero = bl4AroundZero.GetEndPoint(0);

            // Выполняем преобразование, чтобы получить точки не вокруг (0,0,0), а вокруг самого объекта
            LocationPoint pylonLocation = (Pylon.Location as LocationPoint);
            double rotation = pylonLocation.Rotation;

            // Поворот
            Transform transformAroundZ = Transform.CreateRotation(XYZ.BasisZ, rotation);
            Bottom1Pt = transformAroundZ.OfPoint(b1AroundZero);
            Bottom2Pt = transformAroundZ.OfPoint(b2AroundZero);
            Bottom3Pt = transformAroundZ.OfPoint(b3AroundZero);
            Bottom4Pt = transformAroundZ.OfPoint(b4AroundZero);

            // Перемещение
            Transform transformMovement = Transform.CreateTranslation(BottomMiddlePt - XYZ.Zero);
            // Точки вокруг нижней грани объекта по часовой начиная с левого верхнего:
            Bottom1Pt = transformMovement.OfPoint(Bottom1Pt);
            Bottom2Pt = transformMovement.OfPoint(Bottom2Pt);
            Bottom3Pt = transformMovement.OfPoint(Bottom3Pt);
            Bottom4Pt = transformMovement.OfPoint(Bottom4Pt);

            // Получаем верхние точки вокруг объекта
            Transform transformMovementForTop = Transform.CreateTranslation(TopMiddlePt - BottomMiddlePt);
            // Точки вокруг верхней грани объекта по часовой начиная с левого верхнего:
            Top1Pt = transformMovementForTop.OfPoint(Bottom1Pt);
            Top2Pt = transformMovementForTop.OfPoint(Bottom2Pt);
            Top3Pt = transformMovementForTop.OfPoint(Bottom3Pt);
            Top4Pt = transformMovementForTop.OfPoint(Bottom4Pt);
        }
    }
}
