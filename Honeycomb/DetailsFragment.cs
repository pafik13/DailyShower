using System;
using System.Threading;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Content;

using PresenterPlanner.Lib;
using PresenterPlanner.Lib.Doctors;
using PresenterPlanner.Lib.Hospitals;

namespace com.xamarin.sample.fragments.honeycomb
{
    internal class DetailsFragment : Fragment
    {
        public int ShownPlayId { get { return Arguments.GetInt("current_play_id", 0); } }

        public static DetailsFragment NewInstance(int playId)
        {
            var detailsFrag = new DetailsFragment { Arguments = new Bundle() };
            detailsFrag.Arguments.PutInt("current_play_id", playId);
            return detailsFrag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no reason to create our view.
                return null;
            }

            var scroller = new ScrollView(Activity);
			var llayout = new LinearLayout (Activity);
			llayout.Orientation = Orientation.Vertical;
            var text = new TextView(Activity);
            var padding = Convert.ToInt32(TypedValue.ApplyDimension(ComplexUnitType.Dip, 4, Activity.Resources.DisplayMetrics));
            text.SetPadding(padding, padding, padding, padding);
            text.TextSize = 24;
			var doc = DoctorManager.GetDoctor (ShownPlayId);
			var hosp = HospitalManager.GetHospital (doc.HospitalID);
			text.Text = "Фамилия: " + doc.FirstName + "\n\n" +
			"Имя: " + doc.SecondName + "\n\n" +
			"Отчество: " + doc.ThirdName + "\n\n" +
			"Больница: " + hosp.Name + "\n\n" +
			"Адрес: " + hosp.Adress;
				//Shakespeare.Dialogue[ShownPlayId];
			llayout.AddView(text);

			Spinner spn = new Spinner (Activity);

			var preses = Presentations.GetPresentations ();

			List<String> presentsTitle = new List<String>();
			for (int i = 0; i < preses.Count; i++) {
				for (int j = 0; j < preses[i].parts.Count; j++) {
					presentsTitle.Add (preses [i].name + "." + preses [i].parts [j].name);
				}
			}
			spn.Adapter = new ArrayAdapter<String>(Activity, Android.Resource.Layout.SimpleListItemChecked, presentsTitle.ToArray());
			llayout.AddView(spn);

			Button btnSlide = new Button (Activity);
			btnSlide.Text = "Начать показ!";
			btnSlide.Click += (sender, e) => {
				var slides = new Intent ( Activity, typeof(PresentationView));
				int presentationID = 0;
				int partID = spn.SelectedItemPosition;
				for (int i=0; (i <= preses.Count-1) && (partID > preses[i].parts.Count-1); i++){
					presentationID = i+1;
					partID = partID - preses [i].parts.Count;
				}
				slides.PutExtra ("presentationID", presentationID);
				slides.PutExtra ("partID", partID);
				slides.PutExtra ("doctorID", ShownPlayId);
				StartActivity (slides);
			};
			llayout.AddView (btnSlide);

			scroller.AddView (llayout);
            return scroller;
        }
    }
}
