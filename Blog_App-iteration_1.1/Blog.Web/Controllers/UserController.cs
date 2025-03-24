using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace Blog.Web.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        [Route("images/default-profile.jpg")]
        public IActionResult DefaultProfileImage()
        {
            // Create a simple default profile image or return a static image
            // This is a fallback for when the user doesn't have a profile picture
            
            // Return a static image if it exists
            string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string imagePath = Path.Combine(webRootPath, "images", "default-profile.jpg");
            
            if (System.IO.File.Exists(imagePath))
            {
                return PhysicalFile(imagePath, "image/jpg");
            }
            
            // If static image doesn't exist, generate a simple one
            byte[] imageBytes = GenerateDefaultProfileImage();
            return File(imageBytes, "image/jpg");
        }
        
        private byte[] GenerateDefaultProfileImage()
        {
            // This is a simple user profile image (base64 encoded)
            // In a real app, you would generate a proper avatar or use a library to create one
            string base64Image = "iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAYAAADDPmHLAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAOxAAADsQBlSsOGwAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAA/CSURBVHic7Z15cFXVHcc/5+YlBAIkrGGRRZZCQVCQokgRtOJSccOWsTiKS8dqRaBaKtblj9YZtQPtOLQCVmtbFSWMS0cpFhGCuKEFQQREtpCwBsKSjSzvvdM/3rthee/d7b177rv5ztyZZO4953fO+Z7f/f3O2a4wxohkZ6gQwo+wgAYAFh9FAQBaKvDaCMDiI98CWSYTSM8FnEzdHdXbofTQAIC2I5JgARYn7w4YAeDCIzOAE/DV/SbDa06AWp7/yZdGoBp6XDIAhgEaANXAYy7TsowqN6dZYwMaQaM6QJz5JEahBqCJApM6gAGfAY1JTnUu1YURmK4MWlkqL93AZcIE5JdWG4tOAo2FaUChGYDuAApDBYBCANQSUGioBFAIgAYAhaECoDBQA6AwUAOgMFADoDDQQaA4oP9/HcNQsHp59VEWxwk0AjAYYFTvR7mxx5SMniXlFFSc4K3v57PqxAcyXlADQC8MFa8YlDOem3rOpGHW2ZHlGdkNuHPAk1yVexNP7pxIkeOYhAtqDnB8HQDXn3M3vXJGsvnMJg6WfUtFsJySYBmlwTKKHMc4WLYn0smHdR7BtQWTw++XlB9i7vdPUB4sUwOgFzZHX/4c7Zpewv7Sb9hY+BHfnv7C64qJgmHA0Pxf0z3nKraVrGPtiXdrdEINANHomdufvJwCVh9fSVHFEa+rE5NGGc3olTuAfcXb2Fz0oWo5aBgWLGP5wZcp9GjGLRWlwVLmfT+THWfWeV0VvfAyABKLEYFjk+d6wLnZnbmg6XACttR5l+SjV253yoKlfFq4WrUAhNOj4TAGdB7G5Y3OJ2BL/wlvcwZzdediSoPFqgXgJT1yryFgy3C9bzOXEbAlb1TiPdIHwIxuL9K7yUDsScKRy2Zbls0SsMfHR+MJKUN6ALRt2I6RbcdgCY/rHIPedYcTCI9vNZ6A1+PrNckLgBv6TCSrjPDgLDm/3a9Rg9YErFpFV10meQHQMrsVrXPah5dgZX/6XW4gJySI5ATAwI73MSj7WoyM3/5XIXdRlW19NJ5AkgJg7K8epkl288h+PNWN+JrI/u8k2Pk16BFMogDokDuSLo2GImrsrVPX6LX1VxPJGsH15w5jYLNfYwmbxxWKjj6aGgnZMrgZgH65/em//Wl6Ze8A61xshw9CGR+TFb/zG9A1q4BBzW6mYcs+lB05FPnDaQIBr0nWPkAFl599MeNbP4g9jvf7FWZUUq9BU0a0HhvawlVA+gEwpMW99MgdhkW239eV80c537aMgK1aDxqrFo50A6BL40Fc2PJ2zydeaxYG54ymQMafp0qAdAPAMBjS8l5C275ZZf0u0icWZwxpNZbQrp+qGBJ3ApsMgw/2fUFe1jk+6f3VERb4nA7N+pIV6ERF4Rvfa+RvpBoAQhiM63gHjbIaYaSf9h3Cc/+rkhGw02tYrSv9RaYSINkc4EjZMdYcXR9ZznmR+Jv45t5/lsuzRgSsOPZ/z24JkDoAxrW6J7SJkwazfwOD8ufQM3ek19Wwly2FqoQg1QAY3vY2/ylN38gIWMNa3e1tJTSnqSb1HKBw92Gy6+XEPIVjGBYdsofSpMFglbcBE6O641fpR+MJqAuA0a0uC/86uehTTBBs5xiyZwZKJQXJG/3kWyPpnjUs/Mty0O/HcgL2LY0noCQAJrS8I+n7yqOhRc7N9M4e5GkdZJN6DrDs4PrhHz30vviqxDKo+Qz65I73pBKaA1QjJwNtGrfgQMHW0IF0Hw4CFbHI5NK8KXTPvlpJJdQFQEXAYnTXa6gnsrEinZtKzNiXMMo6h+uaz/C9DqoC4IH2kwD/5ABuY3Al/RuPpO85Y5OqhLoAiIdMLhvRYhadG14bORRiSOq8WnRqeBVdG46RUkZdAMyovfO7hUE2c7u9S9v6BTGVUBcAbhDFx16VUT/4Ln3rDUmpFnUBML/mWzk1xTDY3+4tuuaM0RygOj3O6UOXnAFRDuJ2fau3v+4q0TvnJno1uTZOSXUBsGjf8rpT4SQwmNP1/XAZVSf2fA+ABEEI5vc4QM/GY8NnK6qjKgCcQYub249OoqJ1E4Mbmz9EXjZRc4AYtG7UhNx6OUl8wu/EM/ZSvZrBobnrGdX8ibil1AWA2xhMGNyExuQHQr2/boYf5hixbmUUdFnD6Gb3xX1HXQC4iEHzrAIGNLwGq25//QlIK/aCzk/TqcFVcd5RFwCukTDhG9P0QdpY/UNlk+5+dSlxFHR8kK45V8SthroAcBMXhlpD855iePM7PWqTHCM1uLL+BLrmj4pxXV0AuIJbjhcl50oWdD1Ar0buLoFioToAPjpyvKZfqIHxRzOyY28OHthO0RkH+WlZ3MgJeIFhwIou79AxezRGks7vcJw54+TknmPUBcDcDmcDVY50+4h6GZs7/8RrZOeMpVEwjyv2XcLqQwcY3aEHFxX0Suj8Vbn40OuMazZTTQC4RfQkLsrNDdkBHm2+gPtywiNwDzr6lLJwrwGbmc7h5j9w3Z7LWHf0MBM69KF3q3bx73MgOUDCJFzFqpvjjO98IaN7fMDcnNEROQLUGvUBIMTZjaAI0Q97xGJN1zUMyd/gWl/vk3cDlzZvGb634cNcY96eDexrPJwLD/dj+aHdTO5yDQWNm1abxFX7tKoA+O/+/apXMRrGkM4TuLHjWqad1YLQCCTe4C1R55/f7DS3db049M7Zl/1J5BTG4l3w5HZYfhBKKmByx150a94eYfk7B1AVAA+3OgdI8fxckp171UWb6dd6OYvzp2I78jGiG7/D3J5y3m/b77yXERlArG5gCHhuFzyzDbY7KCiHacuhzIGL2/alqHFlmJfLdv4oCuXQNh+9/Rf06XVXKDuSbPxRB3QNc/aGCaH396c0/mqZHvU7dq3zT7BXw5LbFjCh6YyY5dQFgCsYMGlAd67r9hlzGz8W5fxxnD+e88cYQMS636dZNkuGjaFL05YqcwBjcF073jh5aDAZduutbPnD6jKb6eaQ7AE8dPFqJrQYg8MRr/PHeXfHuYzPq0WLehav9g8dMHmy33VMzb8Hw4hdTlUALN63O+EcwN2EL+pOV9+HyC8/9ywFU4/S62d3w7XVYG/O2Q/H9cKCcdl0z2nDqz3fo0fOvfHLRqlNpX5y5Q1FzoiFTv7iDoXqH1dPcn6CxovVR7fAyGXwj63UcP5wD3AIY9D0RnPYcXoNbx3+Z4K1q6TUl9u6kgPc2OZaJrT9Bwsa/y3c+9PheFE+7/pZjYgJMOSc/x9b4C/fQHEFXVdfxYC1w1lw4vN4H4lI1RzgP3lJDEVUBYBLmRwMPmYSHXM6R/X++APGeOUcVdJFyFVfxzV+WsLoLSOZ/eOGGvq/02Hx5jJ48Gt4aL37vb+SIJB/PJwcaVaycsSLdM0eE/dpVQHw+ukhCqeA0HbvnB7P0C5nULg3xzD+uKOI+FVwr7dH37+hZQOWDFtLu+z+RLqoMVgGfHoMZq2DXac82/zBcVa63QffnXiHeYfnYSWQUqoKgAdbNyXhHMCtOYkwGN68B4Pb/YdnG81LYQCZWL6V2PrfXsPoLV3p9mkf9p/Z5Frnrx3VnX/W93DTp/DmTv8eJS+qANKbYXj24KSOU44I06ZC9M27mR7NruOLkrXMPfZSUlduH+/1Z8+/gvUjl9O6fiuEMXjuEMw7CMXlCR3giZFYrYKhs8X9KLk0lE0dEzBIaTdA3gCweWMs+ubd7HsF3CYWjqBDXiUTXt35ZxT2oXfRCGaugnWFifX+yDu1Zf+RYHziNHy4kbwf23PkrP2xD7vGQdoAcELPV0Idd+HI5TUqHjM5vFXB0Z0uYm2P9+icd3tYCVkGJRXwl81w/3IorPnB9mpJPxQKVWEgwIYvX+SFw0+TleD3SCkAnrvscprWz0u6fE1jdeNMxH5uG3Rt/yjrLvyafef9ncLSQ/xtJ8z7rvY9P/x+pFq/v3WCotPfM+/onzCSPIeRUgCM7zUQgfvxmgh+K2Yl+eVWMK7Hhyw6/1uW9ljM0q/O8Pcc6HVBLVYulqylOv+JcphxHHqsvYXFJV9jJTk0l/MXQSWFiCgQQ4E6P+pOgZjPR34xZ5wsILR4jZyYMhweB4VzGuO3r9uyrvQzXj/2FMXlpaEvlcYRrspRrqqTvrK+Wfm7GdN+eOg7h4llrHK1c1QpFb2cpxaOuF0ldF4gvPNlgBAUF2/lts+HcEfrMQxvMtjzK2OjWY8QsLkk9Euhc479b3Lru6TkAI+eXEb3pn1dmfXHS/gcDo7bJSL5gaP6kKbK9bMVVddcq3x+5qMlZ5xe8v2XrD7xJkv3f8xnQ+D8xlJvKQDlVrx27/jPl9CxSasYdUr+82oGgPEZgXCPdBhs/rGQF3afYO+odZ5f9BzLB0uDMO4r+OZH2H52LZ3zEtv9i4WaAEgE6ePHGK8bhGrw03F48BvYUQwHzuxIyfmVBIAQofn87vN7qiucGF7dPROHN/bBxC99fXe4QkoDYOH+b7mqILGrX9zNARJRoDYbQMZjMmvODLjlU/jiJDhsVjw57W2sJGf/1ZEWAIt2bKJrq86eDwLTwY/jabuS01Yw9UP46QxsO72OVzteRH5Wk5Q+ryYAnrjsUk8GgekwB0iEqI6QhnOLn47DzK2wo4hUTv9qoi4A3CYdBZIhXP4g3P8lbDqJa87vkHShTEoBsPTAHq5u1UlODuBWDvBzuXX8uxgvj4Jbv4SPDuOa86c8AAQsObiPLpdE/y6OUU0wG/3b+3lsD787Q9rMtQ1WMGP7ER7YACUVuOb8KQ8Ap8PmqdfHVYeVcBUTiTCNcZ2/PusjY3OoqAKeWs70zWfYVbyeJZ2vIF9S5880AJr++yGWnPmQVw4uoKIizUMsYdC70bU0ye4gZRxTKdMIWMGyHbPZfWpL2OJSY2jAOANIpIzw+7TDNQJWvnc6RgRDn//qwHJWnVrG2wOGckHjc6Xe15VGAFj2y9nw1sFlfFT4BnuLNnFPh9dpJpomXQc3BpCxMVi+6218c2JlpGXScgAhDJxC8KHBH0h0/uhPSM4BorWgspx8Qm/tc9jMPfUYb+5cjuMJgOQCQIjQn11sLlrLF0Vv803xV0zu8Aa52TneRIAAnE5+PrOd5ScW8lnhJ14fHM80ACyH4IPjb7D6xDu8VTCcsW2GE0qJkpMAQnB50yEM7/wWZyqK2HZmLR8cncOR0h+8vkMkMwawncbmzDO8smsXzw4a6nEEQOt6bThcuo/Nx/7HrB1zycu+mFs6PUrzBk3TOg47e0CIsdmw5Tl+OLOHyR2fxXm+yvPOT9UcgPDYLeLUoKPy3eBXsXCE3zc4ePIDXtoxkdzcFtzS6a90bNImEmGS+oMFp8rP8NWJJaw5/i7bT32d3mZ7IjINaJ/bnhs7/omWDTt6GgACi7N/Smb3qY3M++E+CisKsZ0OxnR+gK4tLoyEcaopZGU5Nh8d4MipQ7y49QGOnN6b/v3XTNMgpyV9WvyGvq3HIgzv72fvLPobn+97DsthEQxWMLb7Q/Rt3Z94WUDCeYWAk2eKWbT5eQ4Ub0/vvdHM0qZRF/o0v56eLYZg+OSKueOlR9h+Yi3bT3zF9pNrKK4o9P7eZ0Zptf+XQGZ/QcR38r7+XuMFOgUoDBQACgHQAFAYKAAUBgoAhcH/AdNTybLVPqbyAAAAAElFTkSuQmCC";
            return Convert.FromBase64String(base64Image);
        }
    }
} 