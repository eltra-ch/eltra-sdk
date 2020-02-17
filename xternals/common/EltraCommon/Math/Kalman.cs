namespace EltraCommon.Math
{
    public class Kalman
    {
        #region Private fields

        /* Kalman filter variables */
        private double _q; //process noise covariance
        private double _r; //measurement noise covariance
        private double _p; //estimation error covariance
        
        private double _x; //value        
        private double _k; //kalman gain

        #endregion

        #region Constructors

        public Kalman(double process_noise, double sensor_noise, double estimated_error, double intial_value)
        {
            /* The variables are x for the filtered value, q for the process noise, 
               r for the sensor noise, p for the estimated error and k for the Kalman Gain. 
               The state of the filter is defined by the values of these variables.

               The initial values for p is not very important since it is adjusted
               during the process. It must be just high enough to narrow down.
               The initial value for the readout is also not very important, since
               it is updated during the process.
               But tweaking the values for the process noise and sensor noise
               is essential to get clear readouts.

               For large noise reduction, you can try to start from: (see http://interactive-matter.eu/blog/2009/12/18/filtering-sensor-data-with-a-kalman-filter/ )
               q = 0.125
               r = 32
               p = 1023 //"large enough to narrow down"
               e.g.
               myVar = Kalman(0.125,32,1023,0);
            */
            _q = process_noise;
            _r = sensor_noise;
            _p = estimated_error;
            _x = intial_value; //x will hold the iterated filtered value
        }

        #endregion

        #region Properties

        public double Q => _q;

        public double R => _r;

        public double P => _p;

        #endregion

        #region Methods

        public double GetFilteredValue(double measurement)
        {
            /* Updates and gets the current measurement value */
            //prediction update
            //omit x = x
            _p = _p + _q;

            //measurement update
            _k = _p / (_p + _r);
            _x = _x + _k * (measurement - _x);
            _p = (1 - _k) * _p;

            return _x;
        }

        public void SetParameters(double process_noise, double sensor_noise, double estimated_error)
        {
            _q = process_noise;
            _r = sensor_noise;
            _p = estimated_error;
        }

        public void SetParameters(double process_noise, double sensor_noise)
        {
            _q = process_noise;
            _r = sensor_noise;
        }

        #endregion
    };
}
